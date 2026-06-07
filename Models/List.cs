namespace DictionaryAPI.Models;
using Microsoft.AspNetCore.Identity;

public class List
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = null!;
    public IdentityUser User { get; set; } = null!;
    public DateTime? CreationDate { get; set; } = DateTime.Now;
    public ICollection<WordList> WordLists { get; set; }
    = new List<WordList>();
}
