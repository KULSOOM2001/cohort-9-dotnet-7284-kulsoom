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

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            if (dto == null
                || string.IsNullOrWhiteSpace(dto.Email)
                || string.IsNullOrWhiteSpace(dto.Password)
                || string.IsNullOrWhiteSpace(dto.FullName))
            {
                return null;
            }

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return null;
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
                return null;
            }

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            if (dto == null
                || string.IsNullOrWhiteSpace(dto.Email)
                || string.IsNullOrWhiteSpace(dto.Password))
            {
                return null;
            }

            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return null;
            }

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}