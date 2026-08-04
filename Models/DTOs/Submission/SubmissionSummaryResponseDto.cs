using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.Submission
{
    public record SubmissionSummaryResponseDto
        (
           int Id,
            string UserId,
            string Word,
            DateTime? CreationDate,
            int? StatusId
        );
}
