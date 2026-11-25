using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Reports.PaymentReports;
using Marilog.Contracts.Interfaces.Services.FunctionaltyServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.FunctionaltyServices
{
    public class PdfFileGeneratorService : IPdfFileGeneratorService
    {
        private readonly HttpClient _http;
        public PdfFileGeneratorService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<byte[]> GenerateBillOfLadingFile(int blId, CancellationToken ct = default)
        {
            using var response = await _http.GetAsync($"api/PdfFileGenerator/BillOfLadingPdf/{blId}",
        ct);

            await response.EnsureSuccessWithMessageAsync(ct);

            return await response.Content.ReadAsByteArrayAsync(ct);
        }

        public async Task<byte[]> GenerateDocumentReportPdf(DocumentReport report, string title, CancellationToken ct)
        {
            var url = string.IsNullOrWhiteSpace(title)
         ? "api/PdfFileGenerator/DocumentReport"
         : $"api/PdfFileGenerator/DocumentReport?title={Uri.EscapeDataString(title)}";

            var response = await _http.PostAsJsonAsync(url, report, ct);

            if (!response.IsSuccessStatusCode)
                return [];

            return await response.Content.ReadAsByteArrayAsync(ct);
        }
        public Task<byte[]> GenerateAgingPdf(AgingReport report, CancellationToken ct = default)
            => PostForPdfAsync("api/PdfFileGenerator/Aging", report, ct);

        public Task<byte[]> GenerateStatementPdf(StatementReport report, CancellationToken ct = default)
            => PostForPdfAsync("api/PdfFileGenerator/Statement", report, ct);

        // Posts the report and returns the PDF bytes; shows the server's message on failure.
        private async Task<byte[]> PostForPdfAsync<T>(string url, T report, CancellationToken ct)
        {
            using var response = await _http.PostAsJsonAsync(url, report, ct);
            await response.EnsureSuccessWithMessageAsync(ct);
            return await response.Content.ReadAsByteArrayAsync(ct);
        }
        public async Task<byte[]> GeneratePaymentReportPdf(PaymentsReport report, string title, CancellationToken ct = default)
        {
            var url = string.IsNullOrWhiteSpace(title)
         ? "api/PdfFileGenerator/PaymentReport"
         : $"api/PdfFileGenerator/PaymentReport?title={Uri.EscapeDataString(title)}";

            var response = await _http.PostAsJsonAsync(url, report, ct);

            if (!response.IsSuccessStatusCode)
                return [];

            return await response.Content.ReadAsByteArrayAsync(ct);
        }
    }
}
