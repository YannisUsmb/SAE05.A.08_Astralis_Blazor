using Astralis.Shared.DTOs;

namespace Astralis_BlazorApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> Login(UserLoginDto loginDto);
        Task Logout();
        Task<bool> CheckUserSession();
        Task<AuthResponseDto?> GoogleLogin(GoogleLoginDto googleDto);
        Task<bool> VerifyEmailAsync(string token);
    }
}
