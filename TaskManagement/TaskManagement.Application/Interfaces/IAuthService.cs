using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto dto);
        Task<AuthResult> LoginAsync(LoginDto dto);
    }
}