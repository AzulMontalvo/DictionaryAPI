namespace DictionaryAPI.Models.Entities
{
    public class TermRelation
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public Term Word { get; set; } = null!;
        public int RelatedWordId { get; set; }
        public Term RelatedWord { get; set; } = null!;
        public TermRelationType RelationType { get; set; }
    }
}
