using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private IConfiguration _config;
        private readonly EmailSender _emailSender;
        private readonly TokenService _tokenService;


        public LoginController(ApplicationDbContext context, IConfiguration config, EmailSender emailSender, TokenService tokenService)
        {
            _db = context;
            _config = config;
            _emailSender = emailSender;
            _tokenService=tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO loginRequest)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Username))
                {
                    return BadRequest("Invalid Email or Username");
                }

                UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == loginRequest.Email.ToLower() && x.Username.ToLower() == loginRequest.Username.ToLower());
                if (user != null)
                {
                    user = await user.UpdateUser(user);
                }
                if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email) || user.AccountStatus.Status != "Validated")
                {
                    return NotFound("User Not Found or Invalid Account Status");
                }
                if (user.LoginCodeExp.OrderByDescending(x => x).FirstOrDefault() >= DateTime.UtcNow)
                {
                    return BadRequest("Too early to attempt login again. Please wait awhile before trying again.");
                }

                user.LoginCode = Guid.NewGuid().ToString();
                user.LoginCodeExp.Add(DateTime.UtcNow.AddMinutes(15));
                await _db.SaveChangesAsync();

                bool sendEmail = await SendEmailLogin(user);
                if (sendEmail == true)
                {
                    return Ok($"An Email to complete your login has been sent to {loginRequest.Email}");
                }
                else
                {
                    return StatusCode(500, "Failed to send the login email. Please try again later or contact support for assistance.");
                }
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

                if (user.LoginCodeExp.Any() && user.LoginCodeExp.Last() <= DateTime.UtcNow)
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
