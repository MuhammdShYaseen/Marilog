namespace Marilog.Contracts.DTOs.Responses
{
    public class DocumentTypeResponse
    {
        public int Id { get; set; }
        public string? Code { get;  set; }
        public string? Name { get;  set; }
        public int SortOrder { get;  set; }
    }
}
