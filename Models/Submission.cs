namespace DictionaryAPI.Models;
using Microsoft.AspNetCore.Identity;

public class Submission
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public IdentityUser User { get; set; } = null!;
    public string Word { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? ExtraInformation { get; set; }
    public DateTime? CreationDate { get; set; } = DateTime.Now;
    public SubmissionState StateId { get; set; } = null!;
}
