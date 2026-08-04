using DictionaryAPI.Models.DTOs.Submission;

namespace DictionaryAPI.Interfaces
{
    public interface ISubmissionService
    {
        // User
        Task<IEnumerable<SubmissionSummaryResponseDto>> GetUserSubmissionsAsync();

        // Admin
        Task<IEnumerable<SubmissionSummaryResponseDto>> GetSubmissionsAsync(int? statusId);
        Task<int?> ApproveSubmissionAsync(int submissionId);
        Task<bool> RejectSubmissionAsync(int submissionId);

        // Shared
        Task<SubmissionDetailResponseDto> CreateSubmissionAsync(NewSubmissionDto request);
        Task<SubmissionDetailResponseDto?> GetSubmissionByIdAsync(int submissionId);
    }
}
