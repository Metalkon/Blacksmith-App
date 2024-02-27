using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blacksmith.WebApi.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly TokenService _tokenService;

        public AccountController(ApplicationDbContext context, TokenService tokenService)
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

            if (savedToken.User.AccountStatus.Status == "Banned")
            {
                return StatusCode(403, $"Access Denied: Your account has been permanently banned.");
            }
            if (savedToken.User.AccountStatus.Status == "Suspended")
            {
                return StatusCode(403, $"Access Denied: Your login has been suspended until {savedToken.User.AccountStatus.StatusExp}.");
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
    }
}
