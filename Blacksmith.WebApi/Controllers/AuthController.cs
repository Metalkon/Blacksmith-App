using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared_Classes.Models;
using Blacksmith.WebApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

/*
NOTES:
- Adjust login/register times to include loginstatus which is currently unused.
-
-
-
-
-
*/




namespace Blacksmith.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private IConfiguration _config;
        private readonly IEmailSender _emailSender;

        // Allowed time to login/register before code/url expires
        private int registerTime = 15;

        public AuthController(ApplicationDbContext context, IConfiguration config, IEmailSender emailSender)
        {
            _db = context;  
            _config = config;
            _emailSender = emailSender;
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
                if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email) || user.AccountStatus != "Validated")
                {
                    return NotFound("User Not Found or Invalid Account Status");
                }
                if (user.LoginCodeExp.OrderByDescending(x => x).Count(x => x >= DateTime.UtcNow.AddHours(-24)) > 3)
                {
                    DateTime thirdMostRecent = user.LoginCodeExp.OrderByDescending(x => x).Skip(2).FirstOrDefault();
                    TimeSpan remainingTime = thirdMostRecent.AddHours(24) - DateTime.UtcNow;
                    return BadRequest($"Exceeded the maximum login attempts. Please wait and retry in: {remainingTime}");
                }
                if (user.LoginCodeExp.OrderByDescending(x => x).FirstOrDefault() >= DateTime.UtcNow.AddMinutes(-15))
                {
                    return BadRequest("Too early to attempt login again. Please wait awhile before trying again.");
                }

                user.LoginCode = Guid.NewGuid().ToString();
                user.LoginCodeExp.Add(DateTime.UtcNow);
                await _db.SaveChangesAsync();
                //await SendEmailLogin(user);

                return Ok($"An Email to complete your login has been sent to {loginRequest.Email}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }

        // Complete user login
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
                if (user.LoginCodeExp.Any() && user.LoginCodeExp.Last() <= DateTime.UtcNow)
                {
                    return BadRequest("Your login code has expired, please try to login again");
                }
                if (user.Email == userConfirm.User.Email && user.Username == userConfirm.User.Username && user.LoginCode == userConfirm.Code)
                {
                    user.LoginCode = Guid.NewGuid().ToString();
                    await _db.SaveChangesAsync();
                    RefreshToken newRefreshToken = await GenerateRefreshToken(user);
                    var tokenDTO = new TokenDTO()
                    {
                        RefreshToken = newRefreshToken.Token,
                        Jwt = await GenerateJwt(user)
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

        // Handle user registration and send confirmation email
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(UserDTO registerRequest)
        {
            try
            {
                if (!ModelState.IsValid || registerRequest == null || string.IsNullOrEmpty(registerRequest.Username) || string.IsNullOrEmpty(registerRequest.Email))
                {
                    return BadRequest("Invalid Email or Username");
                }

                UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == registerRequest.Email.ToLower() && x.Username.ToLower() == registerRequest.Username.ToLower());
                if (user == null || user.AccountStatus != "Validated")
                {
                    if (user != null && user.AccountStatus != "Validated" && user.LoginCodeExp.Any() && user.LoginCodeExp.Last() <= DateTime.UtcNow)
                    {
                        return BadRequest($"You are unable to attempt to register again at this time, Please wait and retry in: {registerTime}");
                    }

                    if (user == null)
                    {
                        user = new UserModel()
                        {
                            Email = registerRequest.Email,
                            Username = registerRequest.Username,
                            Role = "None",
                            LoginCode = Guid.NewGuid().ToString(),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            LoginCodeExp = new List<DateTime> { DateTime.UtcNow.AddMinutes(registerTime) }
                        };
                        _db.Users.Add(user);
                    }
                    else
                    {
                        user.Username = registerRequest.Username;
                        user.Role = "None";
                        user.LoginCode = Guid.NewGuid().ToString();
                        user.CreatedAt = DateTime.UtcNow;
                        user.UpdatedAt = DateTime.UtcNow;
                        user.LoginCodeExp.Add(DateTime.UtcNow.AddMinutes(registerTime));
                    }
                    await _db.SaveChangesAsync();
                    await SendEmailRegister(user);
                    return Ok($"A confirmation email has been sent to {user.Email}");
                }
                if (registerRequest.Email == user.Email || registerRequest.Username == user.Username)
                {
                    return BadRequest("Email or Username Has Already Been Taken");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }

        // OLD OUTDATED CODE BELOW

        // Complete user registration
        [AllowAnonymous]
        [HttpPost("register/confirmation")]
        public async Task<ActionResult<string>> RegisterConfirmation(UserConfirmDTO userConfirm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }
            UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == userConfirm.User.Email.ToLower());
            if (user.LoginCodeExp.Any() && user.LoginCodeExp.Last() <= DateTime.UtcNow)
            {
                return BadRequest("The time to confirm your email has expired, please try again");
            }
            // If user information matches with the database, validate account and login the user.
            if (user.Email == userConfirm.User.Email && user.Username == userConfirm.User.Username && user.LoginCode == userConfirm.Code)
            {
                user.Role = "User";
                user.AccountStatus = "Validated";
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                RefreshToken newRefreshToken = await GenerateRefreshToken(user);
                var tokenDTO = new TokenDTO()
                {
                    RefreshToken = newRefreshToken.Token,
                    Jwt = await GenerateJwt(user)
                };
                return Ok(tokenDTO);
            }
            else
            {
                return BadRequest("Invalid Confirmation Data");
            }
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
            string newJwt = await GenerateJwt(savedToken.User);
            return Ok(newJwt);
        }

        // Generate a Refresh Token upon login or registration
        private async Task<RefreshToken> GenerateRefreshToken(UserModel user)
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
        private async Task<string> GenerateJwt(UserModel user)
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

        private async Task SendEmailLogin(UserModel currentUser)
        {
            var subject = "Blacksmith Web App - Login Verification Code";
            var message = $"5 Minute Login URL: \n" +
                $"https://localhost:7001/login/confirmation?id={currentUser.Id}&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginCode}";
            var sendEmail = await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
        }
        private async Task SendEmailRegister(UserModel currentUser)
        {
            var subject = "Blacksmith Web app - Comfirm Registration";
            var message = $"5 Minute Registration URL: \n" +
                $"https://localhost:7001/register/confirmation?id={currentUser.Id}&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginCode}";
            var sendEmail = await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
        }
    }
}
