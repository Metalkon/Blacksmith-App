/*using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Blacksmith.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared_Classes.Models;
using System;
using System.Threading.Tasks;

namespace Blacksmith.WebApi.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _db;
        private readonly EmailSender _emailSender;
        private readonly TokenService _tokenService;

        public RegisterController(AuthService authService, ApplicationDbContext db, EmailSender emailSender, TokenService tokenService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(UserDTO registerRequest)
        {
            try
            {
                var result = await _authService.RegisterUser(registerRequest);

                if (result.StatusCode == 200)
                    return Ok(result.Message);
                else
                    return StatusCode(result.StatusCode, result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("confirmation")]
        public async Task<ActionResult<TokenDTO>> RegisterConfirmation(UserConfirmDTO userConfirm)
        {
            try
            {
                var result = await _authService.ConfirmRegistration(userConfirm);

                if (result.StatusCode == 200)
                    return Ok(result.Token);
                else
                    return StatusCode(result.StatusCode, result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
}*/