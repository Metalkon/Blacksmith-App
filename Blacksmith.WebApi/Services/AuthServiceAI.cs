using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;
using System;
using System.Threading.Tasks;

namespace Blacksmith.WebApi.Services
{
    public class AuthServiceAI
    {
        private readonly ApplicationDbContext _db;
        private readonly EmailSender _emailSender;
        private readonly TokenService _tokenService;

        public AuthServiceAI(ApplicationDbContext db, EmailSender emailSender, TokenService tokenService)
        {
            _db = db;
            _emailSender = emailSender;
            _tokenService = tokenService;
        }

        // Login flow
        public async Task<(int StatusCode, string Message, TokenDTO Token)> LoginUser(UserDTO loginRequest)
        {
            if (!IsUserDtoValid(loginRequest))
                return (400, "Invalid Email or Username", null);

            var user = await FindUserByEmailOrUsername(loginRequest.Email, loginRequest.Username);
            if (user == null)
                return (400, "User Doesn't Exist", null);

            // Check if email/username combination is correct
            if (!DoEmailAndUsernameMatch(user, loginRequest))
                return (400, "Incorrect Email or Username", null);

            user = await user.UpdateUser(user);

            // Check account status
            var statusCheck = CheckAccountStatus(user);
            if (statusCheck.HasRestriction)
                return (statusCheck.StatusCode, statusCheck.Message, null);

            // Check login status
            var loginStatusCheck = await CheckLoginStatus(user);
            if (loginStatusCheck.HasRestriction)
                return (loginStatusCheck.StatusCode, loginStatusCheck.Message, null);

            // If everything is valid, proceed with login
            /*if (user.Validated)
            {
                var emailResult = await PrepareLoginAndSendEmail(user);
                if (emailResult.Success)
                    return (200, $"An Email to complete your login has been sent to {loginRequest.Email}", null);
                else
                    return (500, "Failed to send the login email. Please try again later", null);
            }*/

            return (400, "Access Denied: Your account has not been validated. Please check your email for verification instructions.", null);
        }

        // Registration flow
        public async Task<(int StatusCode, string Message, TokenDTO Token)> RegisterUser(UserDTO registerRequest)
        {
            if (!IsUserDtoValid(registerRequest))
                return (400, "Invalid Email or Username", null);

            var user = await FindUserByEmailOrUsername(registerRequest.Email, registerRequest.Username);

            // Check if user exists and constraints
            if (user != null)
            {
                if (user.Validated || !DoEmailAndUsernameExactlyMatch(user, registerRequest))
                    return (400, "Email or Username has already been taken", null);

                user = await user.UpdateUser(user);

                // Check if user is locked
                var lockCheck = CheckRegistrationLockStatus(user);
                if (lockCheck.HasRestriction)
                    return (lockCheck.StatusCode, lockCheck.Message, null);

                // Check cooldown period
                if (user.LoginCodeExp >= DateTime.UtcNow)
                    return (400, "You are unable to attempt to register again so soon after your previous attempt", null);

                // Update existing unvalidated user
                user.Email = registerRequest.Email;
                user.Username = registerRequest.Username;
                user.LoginStatus = LoginStatus.Awaiting;
                user.LoginAttempts++;
                user.LoginCode = Guid.NewGuid().ToString();
                user.LoginCodeExp = DateTime.UtcNow.AddMinutes(15);
                user.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new user
                user = new UserModel
                {
                    Email = registerRequest.Email,
                    Username = registerRequest.Username,
                    LoginCode = Guid.NewGuid().ToString(),
                    LoginCodeExp = DateTime.UtcNow.AddMinutes(15),
                    LoginStatus = LoginStatus.Awaiting,
                    LoginAttempts = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Users.Add(user);
            }

            await _db.SaveChangesAsync();
            var emailSent = await SendEmailRegister(user);

            if (emailSent)
                return (200, $"A confirmation email has been sent to {user.Email}", null);
            else
                return (500, "Failed to send the registration email. Please try again later", null);
        }

        // Login confirmation flow
        public async Task<(int StatusCode, string Message, TokenDTO Token)> ConfirmLogin(UserConfirmDTO userConfirm)
        {
            if (!IsUserConfirmDtoValid(userConfirm))
                return (400, "Invalid Request", null);

            var user = await FindUserByEmailAndUsername(userConfirm.User.Email, userConfirm.User.Username);
            if (user == null)
                return (400, "User Doesn't Exist", null);

            // Check account status
            var statusCheck = CheckAccountStatus(user);
            if (statusCheck.HasRestriction)
                return (statusCheck.StatusCode, statusCheck.Message, null);

            // Validate login state
            if (user.LoginStatus != LoginStatus.Awaiting)
                return (400, "Your account is not currently awaiting confirmation to login", null);

            if (user.LoginCodeExp <= DateTime.UtcNow)
                return (400, "The time to confirm your email address has expired, please try logging in again", null);

            if (user.LoginCode != userConfirm.Code)
                return (400, "Incorrect Code", null);

            if (user.Validated)
            {
                user.LoginCode = Guid.NewGuid().ToString();
                user.LoginCodeExp = DateTime.UtcNow;
                user.LoginAttempts = 0;
                user.LoginStatus = LoginStatus.Active;

                await _db.SaveChangesAsync();

                var tokenDTO = await GenerateTokens(user);
                return (200, "Login successful", tokenDTO);
            }

            return (400, "Your account has not been validated", null);
        }

        // Registration confirmation flow
        public async Task<(int StatusCode, string Message, TokenDTO Token)> ConfirmRegistration(UserConfirmDTO userConfirm)
        {
            if (!IsUserConfirmDtoValid(userConfirm))
                return (400, "Invalid Request", null);

            var user = await FindUserByEmailAndUsername(userConfirm.User.Email, userConfirm.User.Username);
            if (user == null)
                return (400, "User Doesn't Exist", null);

            if (user.Validated)
                return (400, "Your account has already been validated.", null);

            if (user.LoginStatus != LoginStatus.Awaiting)
                return (400, "Your account is not currently awaiting confirmation to register.", null);

            if (user.LoginCode != userConfirm.Code)
                return (400, "The provided login code is invalid.", null);

            if (user.LoginCodeExp <= DateTime.UtcNow)
                return (400, "The time to confirm your email address has expired, please try registering again", null);

            // Activate the account
            user.Validated = true;
            user.AccountStatus = AccountStatus.Active;
            user.LoginCodeExp = DateTime.UtcNow;
            user.LoginStatus = LoginStatus.Active;
            user.LoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;
            user.CreatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var tokenDTO = await GenerateTokens(user);
            return (200, "Registration successful", tokenDTO);
        }

        #region Helper Methods

        private bool IsUserDtoValid(UserDTO userDto)
        {
            return userDto != null &&
                   !string.IsNullOrEmpty(userDto.Email) &&
                   !string.IsNullOrEmpty(userDto.Username);
        }

        private bool IsUserConfirmDtoValid(UserConfirmDTO userConfirmDto)
        {
            return userConfirmDto != null &&
                   !string.IsNullOrEmpty(userConfirmDto.Code) &&
                   userConfirmDto.User != null &&
                   !string.IsNullOrEmpty(userConfirmDto.User.Email) &&
                   !string.IsNullOrEmpty(userConfirmDto.User.Username);
        }

        private async Task<UserModel> FindUserByEmailOrUsername(string email, string username)
        {
            return await _db.Users.SingleOrDefaultAsync(x =>
                x.Email.ToLower() == email.ToLower() ||
                x.Username.ToLower() == username.ToLower());
        }

        private async Task<UserModel> FindUserByEmailAndUsername(string email, string username)
        {
            return await _db.Users.SingleOrDefaultAsync(x =>
                x.Email.ToLower() == email.ToLower() &&
                x.Username.ToLower() == username.ToLower());
        }

        private bool DoEmailAndUsernameMatch(UserModel user, UserDTO userDto)
        {
            return !(
                (userDto.Email.ToLower() == user.Email.ToLower() && userDto.Username.ToLower() != user.Username.ToLower()) ||
                (userDto.Email.ToLower() != user.Email.ToLower() && userDto.Username.ToLower() == user.Username.ToLower())
            );
        }

        private bool DoEmailAndUsernameExactlyMatch(UserModel user, UserDTO userDto)
        {
            return userDto.Email.ToLower() == user.Email.ToLower() &&
                   userDto.Username.ToLower() == user.Username.ToLower();
        }

        private (bool HasRestriction, int StatusCode, string Message) CheckAccountStatus(UserModel user)
        {
            if (user.AccountStatus == AccountStatus.Banned)
                return (true, 403, "Access Denied: Your account has been permanently banned.");

            if (user.AccountStatus == AccountStatus.Suspended)
                return (true, 403, $"Access Denied: Your login has been suspended until {user.AccountStatusExp}.");

            return (false, 0, null);
        }

        private async Task<(bool HasRestriction, int StatusCode, string Message)> CheckLoginStatus(UserModel user)
        {
            if (user.Validated && user.LoginStatus == LoginStatus.Locked)
            {
                if (user.LoginStatus == LoginStatus.LockedAwaiting)
                {
                    user.LoginStatus = LoginStatus.Locked;
                    user.LoginCode = Guid.NewGuid().ToString();
                    bool sendLockedEmail = await SendEmailLocked(user);

                    if (sendLockedEmail)
                    {
                        await _db.SaveChangesAsync();
                        return (true, 403, "Access Denied: Login with this email address has been locked due to too many failed attempts. An email has been sent containing an 'Unlock' URL if you wish to attempt to login again.");
                    }
                    else
                    {
                        return (true, 500, "Failed to send the email. Please try again later");
                    }
                }

                return (true, 403, "Access Denied: Login with this email address has been locked due to too many failed attempts. To unlock this email address, you will need to confirm ownership by using the 'Unlock' URL sent in the most recently sent email.");
            }

            if (user.Validated && user.LoginCodeExp >= DateTime.UtcNow)
                return (true, 400, "Access Denied: Please wait before attempting to log in again.");

            if (!user.Validated)
                return (true, 400, "Access Denied: Your account has not been validated. Please check your email for verification instructions.");

            return (false, 0, null);
        }

        private (bool HasRestriction, int StatusCode, string Message) CheckRegistrationLockStatus(UserModel user)
        {
            if (user.Validated == false && user.LoginStatus == LoginStatus.Locked)
            {
                if (user.LoginStatus == LoginStatus.LockedAwaiting)
                {
                    // This will be handled in the calling method
                    return (true, 0, null);
                }

                return (true, 403, "Access Denied: Registration with this email address has been locked due to too many failed attempts. To unlock this email address, you will need to confirm ownership by using the 'Unlock' URL sent in the most recently sent email.");
            }

            return (false, 0, null);
        }

        /*private async Task<(bool Success)> PrepareLoginAndSendEmail(UserModel user)
        {
            user.LoginCode = Guid.NewGuid().ToString();
            user.LoginCodeExp = DateTime.UtcNow.AddMinutes(15);
            user.LoginStatus = LoginStatus.Awaiting;
            user.LoginAttempts++;
            await _db.SaveChangesAsync();
            return (await SendEmailLogin(user));
        }*/

        private async Task<bool> SendEmailLogin(UserModel currentUser)
        {
            var subject = "Blacksmith App - Login Verification";
            var message = $"Welcome to Blacksmith Web App!\n\n" +
                          $"To complete your login, click the link below (valid for 15 minutes):\n" +
                          $"https://localhost:8001/confirmation?confirmType=Login&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginCode}";
            return await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
        }

        private async Task<bool> SendEmailRegister(UserModel currentUser)
        {
            var subject = "Blacksmith App - Registration Confirmation";
            var message = $"Welcome to Blacksmith Web App!\n\n" +
                          $"To complete your registration, click the link below (valid for 15 minutes):\n" +
                          $"https://localhost:8001/confirmation?confirmType=Register&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginCode}";
            return await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
        }

        private async Task<bool> SendEmailLocked(UserModel currentUser)
        {
            var subject = "Blacksmith App - Account Has Been Locked";
            var message = $"Login with this email has been locked due to too many failed attempts, if you wish to unlock it and attempt again then click the link below (no expiry time while valid):\n" +
                          $"https://localhost:8001/confirmation?confirmType=UnlockEmail&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginCode}";
            return await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
        }

        private async Task<TokenDTO> GenerateTokens(UserModel user)
        {
            RefreshToken newRefreshToken = await _tokenService.GenerateRefreshToken(user);
            return new TokenDTO
            {
                RefreshToken = newRefreshToken.Token,
                Jwt = await _tokenService.GenerateJwt(user)
            };
        }

        #endregion
    }
}