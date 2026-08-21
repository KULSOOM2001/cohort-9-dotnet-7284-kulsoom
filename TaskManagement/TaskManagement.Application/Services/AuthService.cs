using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<AuthService> logger)
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
            ArgumentNullException.ThrowIfNull(dto);

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                throw new DuplicateEmailException(
                    "Email is already registered.");
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
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _logger.LogWarning(
                    ex,
                    "Registration rejected because the email is already registered");

                throw new DuplicateEmailException(
                    "Email is already registered.",
                    ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to persist new user during registration");

                throw;
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
            ArgumentNullException.ThrowIfNull(dto);

            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return null;
            }

            if (user.PasswordHash.Length != 60 ||
                !user.PasswordHash.StartsWith("$2", StringComparison.Ordinal))
            {
                return null;
            }

            try
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    return null;
                }
            }
            catch (ArgumentException)
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

        private static bool IsUniqueConstraintViolation(
            DbUpdateException exception)
        {
            var message = exception.ToString();

            return message.Contains(
                       "2601",
                       StringComparison.OrdinalIgnoreCase)
                   || message.Contains(
                       "2627",
                       StringComparison.OrdinalIgnoreCase)
                   || message.Contains(
                       "unique",
                       StringComparison.OrdinalIgnoreCase)
                   || message.Contains(
                       "duplicate",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}