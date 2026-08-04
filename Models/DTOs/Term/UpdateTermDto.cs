using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.Term
{
    public class UpdateTermDto
    {
        public int Id { get; set; }
        public string? Word { get; set; } = string.Empty;
        public string? Definition { get; set; } = string.Empty;
        public string? ExtraInformation { get; set; }
        public string? Example { get; set; }
        public string? Etymology { get; set; }
        public TermCategory Category { get; set; }
        public IEnumerable<string>? Tags { get; set; }
        public string? VideoUrl { get; set; }
        public IEnumerable<TermRelationDto>? RelatedTerms { get; set; }
        public DateTime? UpdatedAt { get; set; } = default(DateTime?);
    }
}
