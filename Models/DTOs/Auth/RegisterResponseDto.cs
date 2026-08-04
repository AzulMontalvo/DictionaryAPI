namespace DictionaryAPI.Models.DTOs.Auth
{
    public record RegisterResponseDto
    (
        bool Success,
        AuthResponseDto? Tokens,
        IEnumerable<string>? Errors
    );
}
