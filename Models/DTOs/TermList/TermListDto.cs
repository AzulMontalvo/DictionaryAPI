namespace DictionaryAPI.Models.DTOs.TermList
{
    public class TermListDto
    {
        public int WordId { get; set; }
        public int ListId { get; set; }
        public DateTime? CreationDate { get; set; } = default(DateTime?);
    }
}
