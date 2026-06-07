using System.Net;

namespace DictionaryAPI.Models;
public class WordList
{

    public int WordId { get; set; }
    public Term Word { get; set; } = null!;

    public int ListId { get; set; }
    public List List { get; set; } = null!;

    public DateTime? CreationDate { get; set; } = DateTime.Now;
}
