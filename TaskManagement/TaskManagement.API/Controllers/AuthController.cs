using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            ArgumentNullException.ThrowIfNull(authService);
            ArgumentNullException.ThrowIfNull(logger);

            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Request body is required" });

            var result = await _authService.RegisterAsync(dto);

            if (result == null)
            {
                _logger.LogWarning("Registration failed - email already exists");
                return BadRequest(new { message = "Email already registered" });
            }

            _logger.LogInformation("New user registered successfully");
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Request body is required" });

            var result = await _authService.LoginAsync(dto);

            if (result == null)
            {
                _logger.LogWarning("Failed login attempt");
                return Unauthorized(new { message = "Invalid email or password" });
            }

            _logger.LogInformation("User logged in successfully");
            return Ok(result);
        }
    }
}