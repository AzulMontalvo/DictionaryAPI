namespace DictionaryAPI.Models.DTOs.List
{
    public class FeaturedListResponseDto
    {
        public record FeaturedListScheduleResponseDto(
        int FeaturedId,
        int ListId,
        string ListName,
        DateTime StartDate,
        DateTime EndDate,
        bool IsCurrentlyActive
    );
    }
}
