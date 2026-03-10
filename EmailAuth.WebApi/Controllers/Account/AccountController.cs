using EmailAuth.WebApi.Data;
using EmailAuth.WebApi.Models;
using EmailAuth.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;

namespace EmailAuth.WebApi.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DbContextSqliteUser _db;
        private readonly TokenService _tokenService;

        public AccountController(DbContextSqliteUser context, TokenService tokenService)
        {
            _db = context;
            _tokenService=tokenService;
        }

        // Confirm valid refresh token and then return a new jwt.
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<string>> RefreshTokenConfirmation([FromBody] string refreshToken)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Invalid Request");
            }
            RefreshToken savedToken = await _db.RefreshTokens.Include(x => x.User).SingleOrDefaultAsync(x => x.Token == refreshToken);

            if (savedToken == null)
            {
                return BadRequest("Invalid Token");
            }
            if (savedToken.User.AccountStatus == AccountStatus.Banned)
            {
                return StatusCode(403, $"Access Denied: Your account has been permanently banned.");
            }
            if (savedToken.User.AccountStatus == AccountStatus.Suspended)
            {
                return StatusCode(403, $"Access Denied: Your login has been suspended until {savedToken.User.AccountStatusExp}.");
            }
            if (savedToken == null)
            {
                return BadRequest("Invalid Token");
            }
            if (savedToken.TokenExp <= DateTime.Now)
            {
                return BadRequest("Expired Token");
            }
            string newJwt = await _tokenService.GenerateJwt(savedToken.User);
            return Ok(newJwt);
        }

        // Unlock a users email
        [AllowAnonymous]
        [HttpPost("unlock")]
        public async Task<ActionResult<string>> UnlockConfirmation(UserConfirmDTO userConfirm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }
            UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == userConfirm.User.Email.ToLower() && x.Username.ToLower() == userConfirm.User.Username.ToLower());

            if (user != null) 
            {
                if (user.AccountStatus == AccountStatus.Banned)
                {
                    return StatusCode(403, $"Access Denied: Your account has been permanently banned.");
                }
                if (user.AccountStatus == AccountStatus.Suspended)
                {
                    return StatusCode(403, $"Access Denied: Your login has been suspended until {user.AccountStatusExp}.");
                }
                if (user.LoginStatus == LoginStatus.Locked && user.LockedCode == userConfirm.Code)
                {
                    user.LoginStatus = LoginStatus.Active;
                    user.LockedCode = string.Empty;
                    user.LoginAttempts = 0;
                    await _db.SaveChangesAsync();
                    return Ok("Your Email has been unlocked");
                }
            }
            return BadRequest();
        }

        // Reset Guest Status
        [AllowAnonymous]
        [HttpPost("resetguest")]
        public async Task<ActionResult<AuthResponse>> ResetGuests()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }
            UserModel guestAdmin = await _db.Users.SingleOrDefaultAsync(x => x.Id == 1 && x.Username == "Guest_Admin");
            UserModel guestUser = await _db.Users.SingleOrDefaultAsync(x => x.Id == 2 && x.Username == "Guest_User");

            if (guestAdmin != null && guestUser != null)
            {
                guestAdmin.LoginStatus = LoginStatus.Active;
                guestAdmin.LoginCode = string.Empty;
                guestAdmin.LockedCode = string.Empty;
                guestAdmin.LoginAttempts = 0;
                guestAdmin.Role = "Admin";
                guestAdmin.Validated = true;
                guestAdmin.AccountStatusExp = DateTime.UtcNow;
                guestAdmin.LoginCode = string.Empty;
                guestAdmin.LoginCodeExp = DateTime.UtcNow;

                guestUser.LoginStatus = LoginStatus.Active;
                guestUser.LoginCode = string.Empty;
                guestUser.LockedCode = string.Empty;
                guestUser.LoginAttempts = 0;
                guestUser.Role = "Admin";
                guestUser.Validated = true;
                guestUser.AccountStatusExp = DateTime.UtcNow;
                guestUser.LoginCode = string.Empty;
                guestUser.LoginCodeExp = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return Ok(new AuthResponse 
                {
                    Message = "Guest Accounts Have Been Reset"
                });
            }
            return BadRequest();
        }
    }
}
