using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, ILogger<AuthService> logger)
        {
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(jwtService);
            ArgumentNullException.ThrowIfNull(logger);

            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto dto)
        {
            if (dto == null
                || string.IsNullOrWhiteSpace(dto.Email)
                || string.IsNullOrWhiteSpace(dto.Password)
                || string.IsNullOrWhiteSpace(dto.FullName))
            {
                return AuthResult.Failure(AuthFailureReason.ValidationError, "FullName, Email, and Password are required.");
            }

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return AuthResult.Failure(AuthFailureReason.DuplicateEmail, "Email already registered.");
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User"
            };

            try
            {
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to persist new user during registration");
                return AuthResult.Failure(AuthFailureReason.PersistenceError, "Registration failed due to a server error.");
            }

            var token = _jwtService.GenerateToken(user);

            return AuthResult.SuccessResult(new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            if (dto == null
                || string.IsNullOrWhiteSpace(dto.Email)
                || string.IsNullOrWhiteSpace(dto.Password))
            {
                return AuthResult.Failure(AuthFailureReason.ValidationError, "Email and Password are required.");
            }

            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return AuthResult.Failure(AuthFailureReason.InvalidCredentials, "Invalid email or password.");
            }

            var token = _jwtService.GenerateToken(user);

            return AuthResult.SuccessResult(new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });
        }
    }
}