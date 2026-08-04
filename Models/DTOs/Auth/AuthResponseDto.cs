namespace DictionaryAPI.Models.DTOs.Auth
{
    public record AuthResponseDto
        (
        string Token,
        string RefreshToken,
        DateTime Expiration,
        IList<string> Roles,
        string UserName
        );
}
