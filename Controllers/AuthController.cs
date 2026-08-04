using DictionaryAPI.Interfaces;
using DictionaryAPI.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        //Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var result = await _authService.RegisterAsync(request);
            
            if (!result.Success)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Tokens);
        }

        //Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
                return Unauthorized(new { message = result.Error });
            return Ok(result.Tokens);
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

        //Refresh Token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            if (result == null) return Unauthorized("Invalid token");
            return Ok(result);
        }

        //Revoke Token for Logout
        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            await _authService.RevokeTokenAsync(userId);
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
