using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Services;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;
using Xunit;
namespace TaskManagement.Tests.Auth
{
    public class AuthServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            return new AppDbContext(options);
        }

        private JwtService GetJwtService()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "ThisIsMySuperSecretKeyForJWTTokenGeneration123!"},
                {"Jwt:Issuer", "TaskManagementAPI"},
                {"Jwt:Audience", "TaskManagementClient"}
            };

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            return new JwtService(config);
        }

        [Fact]
        public async Task RegisterAsync_WithNewEmail_ReturnsTokenAndUser()
        {
            var context = GetInMemoryDbContext();
            var userRepository = new UserRepository(context);
            var jwtService = GetJwtService();
            var authService = new AuthService(userRepository, jwtService, NullLogger<AuthService>.Instance);

            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "testuser@example.com",
                Password = "Test123!"
            };

            var result = await authService.RegisterAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("testuser@example.com", result.Email);
            Assert.Equal("User", result.Role); 
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var userRepository = new UserRepository(context);
            var jwtService = GetJwtService();
            var authService = new AuthService(userRepository, jwtService, NullLogger<AuthService>.Instance);

            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "duplicate@example.com",
                Password = "Test123!"
            };

            await authService.RegisterAsync(dto); 

            var result = await authService.RegisterAsync(dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_WithEmptyEmail_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var userRepository = new UserRepository(context);
            var jwtService = GetJwtService();
            var authService = new AuthService(userRepository, jwtService, NullLogger<AuthService>.Instance);

            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "",
                Password = "Test123!"
            };

            var result = await authService.RegisterAsync(dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithCorrectCredentials_ReturnsToken()
        {
            var context = GetInMemoryDbContext();
            var userRepository = new UserRepository(context);
            var jwtService = GetJwtService();
            var authService = new AuthService(userRepository, jwtService, NullLogger<AuthService>.Instance);

            var registerDto = new RegisterDto
            {
                FullName = "Login Test",
                Email = "logintest@example.com",
                Password = "CorrectPassword123!"
            };
            await authService.RegisterAsync(registerDto);

            var loginDto = new LoginDto
            {
                Email = "logintest@example.com",
                Password = "CorrectPassword123!"
            };

            var result = await authService.LoginAsync(loginDto);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var userRepository = new UserRepository(context);
            var jwtService = GetJwtService();
            var authService = new AuthService(userRepository, jwtService, NullLogger<AuthService>.Instance);

            var registerDto = new RegisterDto
            {
                FullName = "Login Test",
                Email = "wrongpass@example.com",
                Password = "CorrectPassword123!"
            };
            await authService.RegisterAsync(registerDto);

            var loginDto = new LoginDto
            {
                Email = "wrongpass@example.com",
                Password = "WrongPassword"
            };

            var result = await authService.LoginAsync(loginDto);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentEmail_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var userRepository = new UserRepository(context);
            var jwtService = GetJwtService();
            var authService = new AuthService(userRepository, jwtService, NullLogger<AuthService>.Instance);

            var loginDto = new LoginDto
            {
                Email = "doesnotexist@example.com",
                Password = "SomePassword123!"
            };

            var result = await authService.LoginAsync(loginDto);

            Assert.Null(result);
        }
    }
}