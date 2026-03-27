namespace Marilog.Presentation.DTOs.DocumentTypeDTOs
{
    public class CreateDocumentTypeRequest
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int SortOrder { get; set; } = 0;
    }
}
