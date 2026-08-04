using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.Term
{
    public record TermDetailPublicResponseDto
    (
        int Id,
        string Word,
        string Definition,
        string? ExtraInformation,
        string? Example,
        string? Etymology,
        TermCategory Category,
        IEnumerable<string> Tags,
        IEnumerable<TermRelationResponseDto> Relations
    );
    public record TermDetailPrivateResponseDto
    (
        int Id,
        string Word,
        string Definition,
        string? ExtraInformation,
        string? Example,
        string? Etymology,
        TermCategory Category,
        IEnumerable<string> Tags,
        IEnumerable<TermRelationResponseDto> Relations,
        DateTime? CreationDate,
        DateTime? UpdatedAt,
        bool IsVisible,
        string? VideoUrl
    );
}
