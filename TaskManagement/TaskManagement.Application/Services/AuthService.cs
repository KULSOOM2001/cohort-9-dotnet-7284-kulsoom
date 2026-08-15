using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services
{
    public class AuthService : IAuthService
    {
        // A fixed, valid BCrypt hash with no matching plaintext password.
        // Used to equalize verification time when no user is found, so that
        // login timing cannot reveal whether an email is registered.
        private const string DummyHashForTimingEquality =
            "$2a$11$CwTycUXWue0Thq9StjUM0uJ8G8vyGX2LG5eD0G4X6q4H1Hqfj9XLK";

        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(jwtService);
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidateRegisterDto(dto);

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

            await _userRepository.AddAsync(user);

            try
            {
                await _userRepository.SaveChangesAsync();
            }
            catch (DuplicateEmailException)
            {
                // Handles the race condition where two requests register the
                // same email concurrently and both pass the initial check above.
                // UserRepository translates the SQL Server IX_Users_Email
                // constraint violation into this Application-level exception;
                // all other DB failures propagate normally.
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
            ArgumentNullException.ThrowIfNull(dto);
            ValidateLoginDto(dto);

            var user = await _userRepository.GetByEmailAsync(dto.Email);

            var passwordHashToVerify = !string.IsNullOrWhiteSpace(user?.PasswordHash)
                ? user.PasswordHash
                : DummyHashForTimingEquality;

            bool passwordIsValid;
            try
            {
                passwordIsValid = BCrypt.Net.BCrypt.Verify(dto.Password, passwordHashToVerify);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Malformed/legacy hash in the database — treat as invalid credentials
                // rather than letting the exception surface as a 500.
                passwordIsValid = false;
            }

            if (user == null || !passwordIsValid)
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

        private static void ValidateRegisterDto(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new ArgumentException("FullName is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters.", nameof(dto));
        }

        private static void ValidateLoginDto(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password is required.", nameof(dto));
        }
    }
}