using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.Interfaces.Services.FunctionaltyServices;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Marilog.Client.Services.FunctionaltyServices
{
    public class PdfFileGeneratorService : IPdfFileGeneratorService
    {
        private readonly HttpClient _http;
        public PdfFileGeneratorService(HttpClient httpClient)
        {
            _http = httpClient;
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
    }
}
