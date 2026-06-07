namespace DictionaryAPI.Models
{
    public class Term
    {
        public int Id { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string? ExtraInformation { get; set; }
        public DateTime? CreationDate { get; set; } = DateTime.Now;
        public ICollection<WordList> WordLists { get; set; }
    = new List<WordList>();
    }
}
