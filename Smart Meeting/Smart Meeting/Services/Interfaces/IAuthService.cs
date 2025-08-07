using SmartMeeting.DTOs;

namespace SmartMeeting.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<bool> LogoutAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<UserInfoDto?> GetUserInfoAsync(string userId);
    }
}
