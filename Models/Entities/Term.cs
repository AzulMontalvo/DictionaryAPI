using DictionaryAPI.Interfaces;

namespace DictionaryAPI.Models.Entities
{
    public class Term : ITrackableEntity
    {
        public int Id { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string? ExtraInformation { get; set; }
        public string? Example { get; set; }
        public string? Etymology { get; set; }
        public TermCategory Category { get; set; }
        public DateTime? CreationDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsVisible { get; set; } = false;
        public string? VideoUrl { get; set; }
        public ICollection<TermList> TermLists { get; set; } = new List<TermList>();
        public ICollection<TermTag> TermTags { get; set; } = new List<TermTag>();
        public ICollection<TermRelation> OutgoingTermRelations { get; set; } = new List<TermRelation>();
        public ICollection<TermRelation> IncomingTermRelations { get; set; } = new List<TermRelation>();
    }
}
