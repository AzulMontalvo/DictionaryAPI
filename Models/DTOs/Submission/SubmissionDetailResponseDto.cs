using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.Submission
{
    public record SubmissionDetailResponseDto
        (
           int Id,
            string UserName,
            string Word,
            string? Definition,
            string? ExtraInformation,
            string? Example,
            string? Etymology,
            TermCategory? Category,
            DateTime? CreationDate,
            int? StatusId
        );
}
