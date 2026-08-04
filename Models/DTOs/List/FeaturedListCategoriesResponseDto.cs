namespace DictionaryAPI.Models.DTOs.List
{
    public record FeaturedListCategoriesResponseDto(
            ListSummaryResponseDto? Starter,
            ListSummaryResponseDto? WeeklyHistory,
            ListSummaryResponseDto? Selection,
            ListSummaryResponseDto? Special
        );
}
