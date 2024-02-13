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
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private IConfiguration _config;
        private readonly EmailSender _emailSender;
        private readonly TokenService _tokenService;

        public AccountController(ApplicationDbContext context, IConfiguration config, EmailSender emailSender, TokenService tokenService)
        {
            _db = context;
            _config = config;
            _emailSender = emailSender;
            _tokenService=tokenService;
        }

        // Confirm valid refresh token and then return a new jwt.
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<string>> RefreshTokenConfirmation(TokenDTO refreshToken)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(refreshToken.RefreshToken))
            {
                return BadRequest("Invalid Request");
            }
            RefreshToken savedToken = await _db.RefreshTokens.Include(x => x.User).SingleOrDefaultAsync(x => x.Token == refreshToken.RefreshToken);

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
