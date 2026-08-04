using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.List
{
    public class NewFeaturedListDto
    {
        public int ListId { get; set; }
        public FeaturedListCategory Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
