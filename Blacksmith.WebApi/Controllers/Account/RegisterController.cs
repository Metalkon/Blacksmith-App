using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;

namespace Blacksmith.WebApi.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _db;
        private readonly TokenService _tokenService;

        public RegisterController(AuthService authService, ApplicationDbContext db, TokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
            _authService = authService;
        }

        // Handle user registration and send confirmation email
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(UserDTO registerRequest)
        {
            try
            {
                if (ModelState.IsValid == false || registerRequest == null)
                    return BadRequest(ModelState);

                // Find users by email and username separately
                var userByEmail = await _db.Users.FirstOrDefaultAsync(x =>
                    x.Email.ToLower() == registerRequest.Email.ToLower());

                var userByUsername = await _db.Users.FirstOrDefaultAsync(x =>
                    x.Username.ToLower() == registerRequest.Username.ToLower());

                // Confirm if the email and username are available
                var confirmUser = await _authService.ConfirmUserRegister(userByEmail, userByUsername);
                if (confirmUser.statusCode != 200)
                    return StatusCode(confirmUser.statusCode, confirmUser.message);

                if (userByEmail == null)
                {
                    userByEmail = new UserModel(); // maybe allow or require input into the constructor for some values like name/email
                    userByEmail.Email = registerRequest.Email;
                    userByEmail.Username = registerRequest.Username;
                    _db.Users.Add(userByEmail);
                }
                else
                {
                    userByEmail = await _authService.CreateUpdateUser(userByEmail, registerRequest);
                }

                // Update the user status on register attempt
                userByEmail = await _authService.UpdateStatus(userByEmail);
                await _db.SaveChangesAsync();

                // Check if a locked email and code needs to be sent
                var lockedEmail = await _authService.LockedEmail(userByEmail);
                if (lockedEmail.statusCode != 200)
                {
                    await _db.SaveChangesAsync();
                    return StatusCode(lockedEmail.statusCode, lockedEmail.message);
                }

                // Initiate registration process
                userByEmail.LoginCode = Guid.NewGuid().ToString();
                userByEmail.LoginCodeExp = DateTime.UtcNow.AddMinutes(1);
                userByEmail.LoginStatus = LoginStatus.Awaiting;
                userByEmail.LoginAttempts++;

                var sendEmail = await _authService.SendEmailRegister(userByEmail);
                if (sendEmail.statusCode != 200)
                    return StatusCode(sendEmail.statusCode, sendEmail.message);

                await _db.SaveChangesAsync();

                return Ok(sendEmail.message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }

        // Complete user registration 
        [AllowAnonymous]
        [HttpPost("confirmation")]
        public async Task<ActionResult<string>> RegisterConfirmation(UserConfirmDTO userConfirm)
        {
            // from login
            try
            {
                if (ModelState.IsValid == false || userConfirm == null)
                    return BadRequest(ModelState);

                // Find the user in the database
                UserModel? user = await _db.Users.SingleOrDefaultAsync(x =>
                x.Email.ToLower() == userConfirm.User.Email.ToLower()
                && x.Username.ToLower() == userConfirm.User.Username.ToLower());

                if (user.Validated)
                    return BadRequest("Email or Username has already been taken");
                if (user.LoginCodeExp <= DateTime.UtcNow)
                    return BadRequest("The time to confirm your email address has expired, please try logging in again");
                if (user.LoginCode != userConfirm.Code)
                    return BadRequest("Incorrect Code");

                // Compelte register process (edit: pasted from old)
                user.Validated = true;
                user.AccountStatus = AccountStatus.Active;
                user.LoginCodeExp = DateTime.UtcNow;
                user.LoginStatus = LoginStatus.Active;
                user.LoginAttempts = 0;
                user.UpdatedAt = DateTime.UtcNow;
                user.CreatedAt = DateTime.UtcNow;

                RefreshToken newRefreshToken = await _tokenService.GenerateRefreshToken(user);
                var tokenDTO = new TokenDTO()
                {
                    RefreshToken = newRefreshToken.Token,
                    Jwt = await _tokenService.GenerateJwt(user)
                };

                await _db.SaveChangesAsync();

                return Ok(tokenDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
}
