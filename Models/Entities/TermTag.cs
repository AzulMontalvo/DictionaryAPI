namespace DictionaryAPI.Models.Entities
{
    public class TermTag
    {
        public int WordId { get; set; }
        public Term Word { get; set; } = null!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
