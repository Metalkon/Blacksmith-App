using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shared_Classes.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Blacksmith.WebApi.Services
{
    public class TokenService
    {
        private readonly ApplicationDbContext _db;
        private IConfiguration _config;
        private readonly EmailSender _emailSender;

        public TokenService(ApplicationDbContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // Generate a Refresh Token upon login or registration
        public async Task<RefreshToken> GenerateRefreshToken(UserModel user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                TokenExp = DateTime.Now.AddDays(30),
                User = user
            };
            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken;
        }

        // Generate a JWT for the provided user object
        public async Task<string> GenerateJwt(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            int expires = 15;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expires),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
