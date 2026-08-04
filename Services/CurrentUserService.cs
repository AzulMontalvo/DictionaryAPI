using DictionaryAPI.Interfaces;
using System.Security.Claims;

namespace DictionaryAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

        public string? UserName =>
            _httpContextAccessor.HttpContext?.User
                .Identity?.Name;

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User
                .Identity?.IsAuthenticated ?? false;

        public bool IsAdmin =>
            _httpContextAccessor.HttpContext?.User
                .IsInRole("Admin") ?? false;
    }
}
