using DictionaryAPI.Models.DTOs.Term;
using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Interfaces
{
    public interface ITermService
    {
        // Public
        Task<TermSummaryPublicResponseDto?> GetDailyWordAsync();
        Task<TermDetailPublicResponseDto?> GetPublicTermByIdAsync(int id);
        Task<IEnumerable<TermSummaryPublicResponseDto>> GetAllPublicTermsAsync();
        Task<IEnumerable<TermSummaryPublicResponseDto>> GetTermsByEtymologyAsync(string keyword);
        Task<IEnumerable<TermSummaryPublicResponseDto>> SearchPublicTermsAsync(string? query, TermCategory? category, string? orderBy);

        //Private
        Task<TermDetailPrivateResponseDto> CreateTermAsync(NewTermDto request);
        Task<TermDetailPrivateResponseDto?> UpdateTermAsync(int id, UpdateTermDto request);
        Task<TermDetailPrivateResponseDto?> GetPrivateTermByIdAsync(int id);
        Task<IEnumerable<TermSummaryPrivateResponseDto>> GetAllPrivateTermsAsync();
        Task<bool> ToggleVisibilityAsync(int id);
        Task<bool> HardDeleteTermAsync(int id);
        Task<IEnumerable<TermSummaryPrivateResponseDto>> SearchPrivateTermsAsync(string? query, TermCategory? category, bool? isVisible, bool? hasVideo, string? orderBy);

        // Shared
        //Task<IEnumerable<TermSummaryPublicResponseDto>> SearchTermsAsync(string query, bool showHidden);

    }
}
