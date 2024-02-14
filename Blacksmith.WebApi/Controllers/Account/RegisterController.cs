﻿using Blacksmith.WebApi.Data;
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
    public class RegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private IConfiguration _config;
        private readonly EmailSender _emailSender;
        private readonly TokenService _tokenService;

        public RegisterController(ApplicationDbContext context, IConfiguration config, EmailSender emailSender, TokenService tokenService)
        {
            _db = context;
            _config = config;
            _emailSender = emailSender;
            _tokenService=tokenService;
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

                UserModel user = await _db.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == registerRequest.Email.ToLower() || x.Username.ToLower() == registerRequest.Username.ToLower());
                if (user != null)
                {
                    if (user.AccountStatus.Validated == true 
                        || (registerRequest.Email == user.Email && registerRequest.Username != user.Username)
                        || (registerRequest.Email != user.Email && registerRequest.Username == user.Username))
                    {
                        return BadRequest("Email or Username has already been taken");
                    }

                    user = await user.UpdateUser(user);

                    if (user.AccountStatus.Validated == false && user.LoginStatus.Status == "Locked")
                    {
                        return StatusCode(403, "Registration with this email address has been Locked due to too many failed attempts. To unlock this email address, you will need to confirm ownership by using the URL sent in the most recent email.");
                    }
                    if (user.AccountStatus.Validated == false && user.LoginCodeExp >= DateTime.UtcNow)
                    {
                        return BadRequest($"You are unable to attempt to register again so soon again after your previous attempt");
                    }
                    if (user.AccountStatus.Validated == false)
                    {
                        user.Email = registerRequest.Email;
                        user.Username = registerRequest.Username;
                        user.LoginCode = Guid.NewGuid().ToString();
                        user.LoginCodeExp = DateTime.UtcNow.AddMinutes(15);
                        user.UpdatedAt = DateTime.UtcNow;
                    }
                }
                if (user == null)
                {
                    user = new UserModel()
                    {
                        Email = registerRequest.Email,
                        Username = registerRequest.Username,
                        LoginCode = Guid.NewGuid().ToString(),
                    };
                    _db.Users.Add(user);
                }
                user.LoginStatus.LoginAttempts++;
                await _db.SaveChangesAsync();
                bool confirmEmail = await SendEmailRegister(user);
                if (confirmEmail == true)
                {
                    return Ok($"A confirmation email has been sent to {user.Email}");
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
            if (user != null)
            {
                user = await user.UpdateUser(user);
            }
            if (user.LoginCodeExp <= DateTime.UtcNow)
            {
                return BadRequest("The time to confirm your email has expired, please try again");
            }
            // If user information matches with the database, validate account and login the user.
            if (user.Email == userConfirm.User.Email && user.Username == userConfirm.User.Username && user.LoginCode == userConfirm.Code)
            {
                user.Role = "User";
                user.AccountStatus.Status = "Validated";
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                RefreshToken newRefreshToken = await _tokenService.GenerateRefreshToken(user);
                var tokenDTO = new TokenDTO()
                {
                    RefreshToken = newRefreshToken.Token,
                    Jwt = await _tokenService.GenerateJwt(user)
                };
                return Ok(tokenDTO);
            }
            else
            {
                return BadRequest("Invalid Confirmation Data");
            }
        }

        private async Task<bool> SendEmailRegister(UserModel currentUser)
        {
            var subject = "Blacksmith Web app - Comfirm Registration";
            var message = $"5 Minute Registration URL: \n" +
                $"https://localhost:7001/register/confirmation?id={currentUser.Id}&username={currentUser.Username}&email={currentUser.Email}&code={currentUser.LoginCode}";
            bool sentEmail = await _emailSender.SendEmailAsync(currentUser.Email, subject, message);
            return sentEmail;
        }

    }
}