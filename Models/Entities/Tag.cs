namespace DictionaryAPI.Models.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<TermTag> TermTags { get; set; } = new List<TermTag>();
    }
}
