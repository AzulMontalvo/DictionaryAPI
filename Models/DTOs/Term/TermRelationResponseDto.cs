using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.Term
{
    public record TermRelationResponseDto
    (
        int RelatedTermId,
        string? RelatedTermWord,
        TermRelationType RelationType
    );
}
