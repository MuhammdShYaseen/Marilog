

using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.DocumentExtraction
{
    public interface IDocumentExtractionService
    {
        Task<IReadOnlyList<ExtractionDebugPage>> DebugAsync(byte[] pdf, CancellationToken ct = default);
        Task<ExtractionResponse> ExtractAsync(IReadOnlyList<ExtractionFile> files, CancellationToken ct = default);
    }
}
