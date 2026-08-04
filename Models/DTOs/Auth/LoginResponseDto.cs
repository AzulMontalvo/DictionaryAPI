namespace DictionaryAPI.Models.DTOs.Auth
{
    public record LoginResponseDto
    (
        bool Success,
        AuthResponseDto? Tokens,
        string? Error
    );
}
