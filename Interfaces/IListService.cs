using DictionaryAPI.Models.DTOs.List;
using static DictionaryAPI.Models.DTOs.List.FeaturedListResponseDto;

namespace DictionaryAPI.Interfaces
{
    public interface IListService
    {
        //Public
        Task<FeaturedListCategoriesResponseDto> GetFeaturedListsAsync();
        Task<IEnumerable<ListSummaryResponseDto>> GetPublicListsAsync();
        //Shared
        Task<ListDetailResponseDto?> GetListByIdAsync(int listId);
        //Needs auth
        Task<IEnumerable<ListSummaryResponseDto>> GetUserListsAsync();
        Task<ListSummaryResponseDto> CreateListAsync(NewListDto dto);
        Task<ListSummaryResponseDto?> RenameListAsync(int listId, string newName);
        Task<bool> DeleteListAsync(int listId);
        Task<bool> AddTermAsync(int listId, int termId);
        Task<bool> RemoveTermAsync(int listId, int termId);
        // Admin
        Task<bool> CreateScheduledListAsync(NewFeaturedListDto dto);
        Task<IEnumerable<FeaturedListScheduleResponseDto>> GetFeaturedScheduleAsync();
    }
}
