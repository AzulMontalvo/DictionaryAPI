using DictionaryAPI.Models.Entities;

namespace DictionaryAPI.Models.DTOs.Submission
{
    public class NewSubmissionDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Word { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string? ExtraInformation { get; set; }
        public string? Example { get; set; }
        public string? Etymology { get; set; }
        public TermCategory Category { get; set; }
        public DateTime? CreationDate { get; set; } = DateTime.Now;
        public int StatusId { get; set; }
    }
}
