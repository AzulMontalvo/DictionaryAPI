namespace DictionaryAPI.Models.Entities;

using Microsoft.AspNetCore.Identity;

public class List
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description {get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public bool IsPublic { get; set; } = false;
    public DateTime? CreationDate { get; set; } = DateTime.Now;
    public ICollection<TermList> TermLists { get; set; }
    = new List<TermList>();
}
