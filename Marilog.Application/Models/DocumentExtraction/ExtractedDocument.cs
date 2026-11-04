using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Models.DocumentExtraction
{
    public sealed class ExtractedDocument
    {
        public List<int> Pages { get; } = [];
        public List<TextLine> Lines { get; } = [];
        public ExtractedHeader Header { get; set; } = default!;
    }
}
