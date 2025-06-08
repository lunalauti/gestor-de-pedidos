using Microsoft.AspNetCore.Mvc;

using Backend.Modules.Users.Application.Services;
using Backend.Modules.Users.Application.DTOs;

namespace Backend.Modules.Users.Presentation 
{
   
    [Route("api/user")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);

            if (response.Message == "User already exists")
                return BadRequest(new { response.Message });

            return Ok(new { response.Message });
        }
    }
}
