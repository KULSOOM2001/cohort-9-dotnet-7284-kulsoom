using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
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
            {
                return BadRequest(new
                {
                    message = "Request body is required"
                });
            }

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Registration failed: {Reason}",
                    result.FailureReason);

                return result.FailureReason switch
                {
                    AuthFailureReason.ValidationError =>
                        BadRequest(new
                        {
                            message = result.ErrorMessage
                        }),

                    AuthFailureReason.DuplicateEmail =>
                        Conflict(new
                        {
                            message = result.ErrorMessage
                        }),

                    _ =>
                        StatusCode(500, new
                        {
                            message = "An unexpected error occurred."
                        })
                };
            }

            _logger.LogInformation(
                "New user registered successfully");

            return Ok(result.Data);
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    message = "Request body is required"
                });
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Failed login attempt");

                return Unauthorized(new
                {
                    message = result.ErrorMessage
                });
            }

            _logger.LogInformation(
                "User logged in successfully");

            return Ok(result.Data);
        }
    }
}