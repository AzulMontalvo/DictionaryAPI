namespace DictionaryAPI.Models.DTOs.List
{
    public class NewListDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
    }
}
