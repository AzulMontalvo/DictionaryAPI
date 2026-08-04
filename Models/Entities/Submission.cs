namespace DictionaryAPI.Models.Entities;

using Microsoft.AspNetCore.Identity;
using DictionaryAPI.Interfaces;

public class Submission : ITrackableEntity
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public string Word { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? ExtraInformation { get; set; }
    public string? Example { get; set; }
    public string? Etymology { get; set; }
    public TermCategory Category { get; set; } = TermCategory.None;
    public DateTime? CreationDate { get; set; } = DateTime.Now;
    public int StatusId { get; set; } = 1;
    public SubmissionStatus Status { get; set; } = null!;
    public DateTime? UpdatedAt { get; set; }
}
