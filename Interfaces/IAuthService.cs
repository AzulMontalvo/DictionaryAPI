using DictionaryAPI.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace DictionaryAPI.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterDto request);
        Task<LoginResponseDto> LoginAsync(LoginDto request);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto request);
        Task RevokeTokenAsync(string userId);
    }
}
