﻿using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared_Classes.Models;
using System.Security.Claims;

namespace Blacksmith.WebApi.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DbContextSqlServer _db;
        private readonly TokenService _tokenService;

        public AccountController(DbContextSqlServer context, TokenService tokenService)
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
                    user.LockedCode = null;
                    user.LoginAttempts = 0;
                    await _db.SaveChangesAsync();
                    return Ok("Your Email has been unlocked");
                }
            }
            return BadRequest();
        }

        [AllowAnonymous]
        [HttpGet("PlayerData")]
        public async Task<ActionResult<string>> PlayerDisplayData()
        {
            string userEmail = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            UserModel user = await _db.Users.Include(u => u.GameData).SingleOrDefaultAsync(x => x.Email == userEmail);

            if (user != null)
            {
                PlayerDataDTO playerData = new PlayerDataDTO()
                {
                    Username = user.Username ?? "N/A",
                    Level = user.GameData.Level,
                    Experience = user.GameData.Experience,
                    Schematics = 1, //Schematics = user.Data.Inventory.Count(x => x.Type == "Schematic"),
                    Gold = user.GameData.Gold
                };

                return Ok(playerData);
            }
            return BadRequest();
        }
    }
}
