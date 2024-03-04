using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

namespace Blacksmith.WebApi.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly EmailSender _emailSender;
        private readonly TokenService _tokenService;

        public LoginController(ApplicationDbContext context, EmailSender emailSender, TokenService tokenService)
        {
            _db = context;
            _emailSender = emailSender;
            _tokenService=tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO loginRequest)
        {
            try
            {
                if (!ModelState.IsValid || loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Username))
                {
                    return BadRequest("Invalid Email or Username");
                }

                UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == loginRequest.Email.ToLower() || x.Username.ToLower() == loginRequest.Username.ToLower());
                if (user != null)
                {
                    if ((loginRequest.Email.ToLower() == user.Email.ToLower() && loginRequest.Username.ToLower() != user.Username.ToLower())
                        || (loginRequest.Email.ToLower() != user.Email.ToLower() && loginRequest.Username.ToLower() == user.Username.ToLower()))
                    {
                        return BadRequest("Incorrect Email or Username");
                    }

                    user = await user.UpdateUser(user);

                    if (user.AccountStatus.Status == "Banned")
                    {
                        return StatusCode(403, $"Access Denied: Your account has been permanently banned.");
                    }
                    if (user.AccountStatus.Status == "Suspended")
                    {
                        return StatusCode(403, $"Access Denied: Your login has been suspended until {user.AccountStatus.StatusExp}.");
                    }
                    if (user.AccountStatus.Validated && user.LoginStatus.Status.Contains("Locked"))
                    {
                        if (user.LoginStatus.Status == "Locked/Awaiting")
                        {
                            bool sendLockedEmail = await SendEmailLocked(user);
                            if (sendLockedEmail == true)
                            {
                                user.LoginStatus.Status = "Locked";
                                return StatusCode(403, "Access Denied: Login with this email address has been locked due to too many failed attempts. An email has been sent containing an 'Unlock' URL if you wish to attempt to login again.");
                            }
                            else
                            {
                                return StatusCode(500, "Failed to send the email. Please try again later");
                            }
                        }
                        return StatusCode(403, "Access Denied: Login with this email address has been locked due to too many failed attempts. To unlock this email address, you will need to confirm ownership by using the 'Unlock' URL sent in the most recently sent email.");
                    }
                    if (user.AccountStatus.Validated && user.LoginCodeExp >= DateTime.UtcNow)
                    {
                        return BadRequest($"Access Denied: Please wait before attempting to log in again.");
                    }
                    if (user.AccountStatus.Validated == false)
                    {
                        return BadRequest("Access Denied: Your account has not been validated. Please check your email for verification instructions.");
                    }
                    if (user.AccountStatus.Validated == true && user.Email.ToLower() == loginRequest.Email.ToLower() && user.Username.ToLower() == loginRequest.Username.ToLower())
                    {
                        user.LoginCode = Guid.NewGuid().ToString();
                        user.LoginCodeExp = DateTime.UtcNow.AddMinutes(15);
                        user.LoginStatus.Status = "Awaiting";

                        await _db.SaveChangesAsync();
                        bool sendEmail = await SendEmailLogin(user);

                        if (sendEmail == true)
                        {
                            return Ok($"An Email to complete your login has been sent to {loginRequest.Email}");
                        }
                        if (sendEmail == false)
                        {
                            return StatusCode(500, "Failed to send the login email. Please try again later");
                        }
                    }
                }
                if (user == null)
                {
                    return BadRequest("User Doesn't Exist");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("confirmation")]
        public async Task<ActionResult<TokenDTO>> LoginConfirmation(UserConfirmDTO userConfirm)
        {
            try
            {
                if (!ModelState.IsValid || userConfirm == null || string.IsNullOrEmpty(userConfirm.Code) || string.IsNullOrEmpty(userConfirm.User.Username) || string.IsNullOrEmpty(userConfirm.User.Email))
                {
                    return BadRequest("Invalid Request");
                }

                UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == userConfirm.User.Email.ToLower() && x.Username.ToLower() == userConfirm.User.Username.ToLower());

                if (user != null)
                {
                    if (user.AccountStatus.Status == "Banned")
                    {
                        return StatusCode(403, $"Access Denied: Your account has been permanently banned.");
                    }
                    if (user.AccountStatus.Status == "Suspended")
                    {
                        return StatusCode(403, $"Access Denied: Your login has been suspended until {user.AccountStatus.StatusExp}.");
                    }
                    if (user.LoginStatus.Status != "Awaiting")
                    {
                        return BadRequest("Your account is not currently awaiting confirmation to login");
                    }
                    if (user.LoginCodeExp <= DateTime.UtcNow)
                    {
                        return BadRequest("The time to confirm your email address has expired, please try logging in again");
                    }
                    if (user.LoginCode != userConfirm.Code)
                    {
                        return BadRequest("Incorrect Code");
                    }
                    if (user.AccountStatus.Validated == true)
                    {
                        user.LoginCode = Guid.NewGuid().ToString();
                        user.LoginCodeExp = DateTime.UtcNow;
                        user.LoginStatus.LoginAttempts = 0;
                        user.LoginStatus.Status = "Active";

                        await _db.SaveChangesAsync();

                        RefreshToken newRefreshToken = await _tokenService.GenerateRefreshToken(user);
                        var tokenDTO = new TokenDTO()
                        {
                            RefreshToken = newRefreshToken.Token,
                            Jwt = await _tokenService.GenerateJwt(user)
                        };
                        return Ok(tokenDTO);
                    }
                }               
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }

        private async Task<bool> SendEmailLogin(UserModel currentUser)
        {
            var subject = "Blacksmith App - Login Verification";
            var message = $"Welcome to Blacksmith Web App!\n\n" +
                          $"To complete your login, click the link below (valid for 15 minutes):\n" +
                          $"https://localhost:8001/confirmation?confirmType=Login&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginCode}";
            bool sentEmail = await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
            return sentEmail;
        }

        private async Task<bool> SendEmailLocked(UserModel currentUser)
        {
            var subject = "Blacksmith App - Account Has Been Locked";
            var message = $"Login with this email has been locked due to too many failed attempts, if you wish to unlock it and attempt again then click the link below (no expiry time while valid):\n" +
                          $"https://localhost:8001/confirmation?confirmType=Locked&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginStatus.StatusCode}";
            bool sentEmail = await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
            return sentEmail;
        }
    }
}
