using DictionaryAPI.Models.DTOs.Term;
using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.List
{
    public record ListSummaryResponseDto
    (
        int Id,
        string Name,
        int TermCount,
        string? Description,
        DateTime? CreationDate
    );

    public record ListDetailResponseDto
    (
        int Id,
        string Name,
        int TermCount,
        string? Description,
        DateTime? CreationDate,
        IEnumerable<TermSummaryPublicResponseDto> Terms
    );
}
