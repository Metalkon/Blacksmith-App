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

                UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == loginRequest.Email.ToLower() && x.Username.ToLower() == loginRequest.Username.ToLower());
                if (user != null)
                {
                    user = await user.UpdateUser(user);

                    if (user.AccountStatus.Status == "Banned")
                    {
                        return StatusCode(403, $"Access Denied: Your account has been permanently banned.");
                    }
                    if (user.AccountStatus.Status == "Suspended")
                    {
                        return StatusCode(403, $"Access Denied: Your login has been suspended until {user.AccountStatus.StatusExp}.");
                    }
                    if (user.AccountStatus.Validated && user.LoginStatus.Status == "Locked")
                    {
                        return StatusCode(403, "Access Denied: Login with this email address has been Locked due to too many failed attempts. To unlock this email address, you will need to confirm ownership by using the \"Unlock\" URL sent in the most recently sent email.");
                    }
                    if (user.AccountStatus.Validated && user.LoginCodeExp >= DateTime.UtcNow)
                    {
                        return BadRequest($"Access Denied: Please wait before attempting to log in again.");
                    }
                    if (user.AccountStatus.Validated == false)
                    {
                        return BadRequest("Access Denied: Your account has not been validated. Please check your email for verification instructions.");
                    }
                    if (user.AccountStatus.Validated == true)
                    {
                        user.LoginCode = Guid.NewGuid().ToString();
                        user.LoginCodeExp =DateTime.UtcNow.AddMinutes(15);

                        await _db.SaveChangesAsync();
                        bool sendEmail = await SendEmailLogin(user);

                        if (sendEmail == true)
                        {
                            return Ok($"An Email to complete your login has been sent to {loginRequest.Email}");
                        }
                    }
                    else
                    {
                        return StatusCode(500, "Failed to send the login email. Please try again later or contact support for assistance.");
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
        [HttpPost("login/confirmation")]
        public async Task<ActionResult<TokenDTO>> LoginConfirmation(UserConfirmDTO userConfirm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Request");
                }
                UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == userConfirm.User.Email.ToLower() && x.Username.ToLower() == userConfirm.User.Username.ToLower());
                user = await user.UpdateUser(user);

                if (user.LoginCodeExp <= DateTime.UtcNow)
                {
                    return BadRequest("Your login code has expired, please try to login again");
                }
                if (user.Email == userConfirm.User.Email && user.Username == userConfirm.User.Username && user.LoginCode == userConfirm.Code)
                {
                    user.LoginCode = Guid.NewGuid().ToString();
                    await _db.SaveChangesAsync();
                    RefreshToken newRefreshToken = await _tokenService.GenerateRefreshToken(user);
                    var tokenDTO = new TokenDTO()
                    {
                        RefreshToken = newRefreshToken.Token,
                        Jwt = await _tokenService.GenerateJwt(user)
                    };
                    return tokenDTO;
                }
                else
                {
                    return BadRequest("Invalid Login Data");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }
        private async Task<bool> SendEmailLogin(UserModel currentUser)
        {
            var subject = "Blacksmith Web App - Login Verification Code";
            var message = $"5 Minute Login URL: \n" +
                $"https://localhost:7001/login/confirmation?id={currentUser.Id}&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginCode}";
            bool sentEmail = await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
            return sentEmail;
        }
    }
}
