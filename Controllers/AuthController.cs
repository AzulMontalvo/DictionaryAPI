using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DictionaryAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        //Helper method
        private void SetRefreshTokenCookie(string refreshToken, DateTime expiry)
        {
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = new DateTimeOffset(expiry)
            });
        }

        //Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success) return BadRequest(new { errors = result.Errors });

            SetRefreshTokenCookie(result.Tokens!.RefreshToken, result.Tokens.RefreshTokenExpiry);
            return Ok(new
            {
                result.Tokens.Token,
                result.Tokens.Expiration,
                result.Tokens.Roles,
                result.Tokens.UserName
            });
        }

        //Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success) return Unauthorized(new { message = result.Error });

            SetRefreshTokenCookie(result.Tokens!.RefreshToken, result.Tokens.RefreshTokenExpiry);
            return Ok(new
            {
                result.Tokens.Token,
                result.Tokens.Expiration,
                result.Tokens.Roles,
                result.Tokens.UserName
            });
        }

        //Confirm email
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Parámetros inválidos." });

            var confirmed = await _authService.ConfirmEmailAsync(userId, token);

            return confirmed
                ? Ok(new { message = "Correo confirmado correctamente." })
                : BadRequest(new { message = "El enlace no es válido o expiró." });
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto request)
        {
            await _authService.ResendConfirmationEmailAsync(request.Email);

            return Ok(new { message = "Si el correo existe y no está confirmado, recibirás un nuevo enlace." });
        }

        //Refresh Token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "No hay sesión activa." });

            var tokens = await _authService.RefreshTokenAsync(refreshToken);
            if (tokens is null)
            {
                Response.Cookies.Delete("refresh_token");
                return Unauthorized(new { message = "Sesión expirada." });
            }

            SetRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiry);
            return Ok(new
            {
                tokens.Token,
                tokens.Expiration,
                tokens.Roles,
                tokens.UserName
            });
        }

        //Revoke Token for Logout
        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            await _authService.RevokeTokenAsync(userId);
            Response.Cookies.Delete("refresh_token");

            return NoContent();
        }

        // For testing protected endpoint
        [Authorize]
        [HttpGet("protected")]
        public IActionResult Me()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            return Ok(new { email, roles });
        }
    }
}
