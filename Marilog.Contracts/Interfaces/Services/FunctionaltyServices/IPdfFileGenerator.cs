using Marilog.Contracts.DTOs.Reports.DocumentReports;

namespace Marilog.Contracts.Interfaces.Services.FunctionaltyServices
{
    public interface IPdfFileGenerator
    {
        byte[] GenerateDocumentReportPdf(DocumentReport report, string title);
    }
}
