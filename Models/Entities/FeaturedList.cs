namespace DictionaryAPI.Models.Entities
{
    public class FeaturedList
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public List List { get; set; } = null!;
        public FeaturedListCategory Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
