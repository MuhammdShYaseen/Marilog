using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.FunctionaltyServices
{
    public interface IPdfFileGeneratorService
    {
        Task<byte[]> GenerateDocumentReportPdf(DocumentReport report, string title, CancellationToken ct =default);
        Task<byte[]> GenerateBillOfLadingFile(BillOfLadingResponse bl, CancellationToken ct = default);
    }
}
