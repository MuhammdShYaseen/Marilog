namespace Marilog.Contracts.DTOs.Requests.DocumentTypeDTOs
{
    public class UpdateDocumentTypeRequest
    {
        public string Name { get; set; } = default!;
        public int SortOrder { get; set; }
    }
}
