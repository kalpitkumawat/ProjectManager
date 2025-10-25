using ProjectManager.Api.DTOs;

namespace ProjectManager.Api.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    string GenerateJwtToken(int userId, string username, string email);
}