using Marilog.Contracts.DTOs.Reports.DocumentReports;

namespace Marilog.Contracts.Interfaces.Services.FunctionaltyServices
{
    public interface IPdfFileGeneratorService
    {
        Task<byte[]> GenerateDocumentReportPdf(DocumentReport report, string title, CancellationToken ct =default);
    }
}
