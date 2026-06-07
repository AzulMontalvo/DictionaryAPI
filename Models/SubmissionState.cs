namespace DictionaryAPI.Models
{
    public class SubmissionState
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    = new List<Submission>();
    }
}
