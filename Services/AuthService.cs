using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.Auth;
using DictionaryAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DictionaryAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        //Register
        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto request)
        {
            var user = new AppUser { UserName = request.Username, Email = request.Email };
            try
            {
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return new RegisterResponseDto(false, null, errors);
                }

                await _userManager.AddToRoleAsync(user, "User");

                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = Uri.EscapeDataString(token);
                    var frontendUrl = _configuration["App:FrontendUrl"];
                    var confirmationLink = $"{frontendUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

                    await _emailService.SendEmailConfirmationAsync(user.Email!, user.UserName!, confirmationLink);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando email de confirmación: {ex.Message}");
                }
                var tokens = await GenerateJwtTokenAsync(user);

                return new RegisterResponseDto(true, tokens, null);
            }
            catch (Exception)
            {
                return new RegisterResponseDto(false, null, new[] { "An error occurred during registration. Please try again." });
            }
        }

        //Login
        public async Task<LoginResponseDto> LoginAsync(LoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return new LoginResponseDto(false, null, "Credenciales inválidas.");

            var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!validPassword)
                return new LoginResponseDto(false, null, "Credenciales inválidas.");

            if (!user.EmailConfirmed)
                return new LoginResponseDto(false, null, "Debes confirmar tu correo antes de iniciar sesión.");

            var tokens = await GenerateJwtTokenAsync(user);
            return new LoginResponseDto(true, tokens, null);
        }

        //Confirm email
        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;

            var decodedToken = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            return result.Succeeded;
        }

        //Refresh Token
        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto request)
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null) return null;

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            return await GenerateJwtTokenAsync(user);
        }

        //Revoke Token for Logout
        public async Task RevokeTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
        }

        //Helper Methods
        private async Task<AuthResponseDto> GenerateJwtTokenAsync(AppUser user)
        {
            var expiry = DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:DurationInMinutes"));

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = GenerateJwtToken(user, expiry, roles);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:DurationInDays"));

            await _userManager.UpdateAsync(user);
            return new AuthResponseDto(Token: accessToken, RefreshToken: refreshToken, Expiration: expiry, roles, user.UserName!);
        }

        private string GenerateJwtToken(AppUser user, DateTime expiry, IList<string> roles)
        {
            var userRole = roles.FirstOrDefault() ?? "User";

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id),
                new (ClaimTypes.Email, user.Email!),
                new (ClaimTypes.Role, userRole),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false
            };

            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, tokenValidationParams, out var validatedToken);
                if (validatedToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                    return null;
                return principal;
            }
            catch
            {
                return null;
            }
        }
}
}
