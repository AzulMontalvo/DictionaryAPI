using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.Term
{
        public record TermSummaryPublicResponseDto
        (
            int Id,
            string Word,
            string Definition,
            string? Etymology,
            TermCategory Category
        );

        public record TermSummaryPrivateResponseDto
        (
            int Id,
            string Word,
            string Definition,
            TermCategory Category,
            bool IsVisible,
            DateTime? CreationDate
        );
}
