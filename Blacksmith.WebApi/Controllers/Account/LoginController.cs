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
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _db;
        private readonly TokenService _tokenService;

        public LoginController(AuthService authService, ApplicationDbContext db, TokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO loginRequest)
        {
            try
            {
                if (ModelState.IsValid == false || loginRequest == null)
                    return BadRequest(ModelState);

                // Find the user in the database
                UserModel? user = await _db.Users.FirstOrDefaultAsync(x => 
                    x.Email.ToLower() == loginRequest.Email.ToLower() 
                    || x.Username.ToLower() == loginRequest.Username.ToLower());

                // Confirm if the user exists/matches
                var confirmUser = await _authService.ConfirmUser(user, loginRequest);
                if (confirmUser.statusCode != 200) 
                    return StatusCode(confirmUser.statusCode, confirmUser.message);

                // Update the user status on login attempt
                user = await _authService.UpdateStatus(user);
                await _db.SaveChangesAsync();

                // Check if the user is allowed to login
                var checkUser = await _authService.CheckUserStatus(user);
                if (checkUser.statusCode != 200)
                    return StatusCode(checkUser.statusCode, checkUser.message);

                // Check if a locked email and code needs to be sent
                var lockedEmail = await _authService.LockedEmail(user);
                if (lockedEmail.statusCode != 200)
                {
                    await _db.SaveChangesAsync();
                    return StatusCode(lockedEmail.statusCode, lockedEmail.message);
                }

                // Initiate login process
                user.LoginCode = Guid.NewGuid().ToString();
                user.LoginCodeExp = DateTime.UtcNow.AddMinutes(15);
                user.LoginStatus = LoginStatus.Awaiting;
                user.LoginAttempts++;

                var sendEmail = await _authService.SendEmailLogin(user);
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
        
        [AllowAnonymous]
        [HttpPost("confirmation")]
        public async Task<ActionResult<TokenDTO>> LoginConfirmation(UserConfirmDTO userConfirm)
        {
            try
            {
                if (ModelState.IsValid == false || userConfirm == null)
                    return BadRequest(ModelState);               

                // Find the user in the database
                UserModel? user = await _db.Users.SingleOrDefaultAsync(x => 
                x.Email.ToLower() == userConfirm.User.Email.ToLower() 
                && x.Username.ToLower() == userConfirm.User.Username.ToLower());

                // Confirm if the user exists/matches
                var confirmUser = await _authService.ConfirmUser(user, userConfirm.User);
                if (confirmUser.statusCode != 200)
                    return StatusCode(confirmUser.statusCode, confirmUser.message);

                if (user.LoginCodeExp <= DateTime.UtcNow)
                    return BadRequest("The time to confirm your email address has expired, please try logging in again");
                if (user.LoginCode != userConfirm.Code)
                    return BadRequest("Incorrect Code");

                // Compelte login process
                user.LoginCode = Guid.NewGuid().ToString();
                user.LoginCodeExp = DateTime.UtcNow;
                user.LoginAttempts = 0;
                user.LoginStatus = LoginStatus.Active;

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