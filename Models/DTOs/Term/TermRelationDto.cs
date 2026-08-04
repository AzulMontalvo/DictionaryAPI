using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.Term
{
    public class TermRelationDto
    {
        public int RelatedTermId { get; set; }
        public TermRelationType RelationType { get; set; }
    }
}
