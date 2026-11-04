using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.DocumentExtraction;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class DocumentExtractionService : IDocumentExtractionService
    {
        private readonly HttpClient _http;

        public DocumentExtractionService(HttpClient http) => _http = http;

        public async Task<ExtractionResponse> ExtractAsync(IReadOnlyList<ExtractionFile> files, CancellationToken ct = default)
        {
            using var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                var part = new ByteArrayContent(file.Content);
                part.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content.Add(part, "files", file.FileName);   // "files" = the controller parameter name
            }

            var response = await _http.PostAsync("api/documents/extract", content, ct);
            await response.EnsureSuccessWithMessageAsync();
            return (await response.Content.ReadFromJsonAsync<ExtractionResponse>(cancellationToken: ct))!;
        }

        public async Task<IReadOnlyList<ExtractionDebugPage>> DebugAsync(byte[] pdf, CancellationToken ct = default)
        {
            using var content = new MultipartFormDataContent();
            var part = new ByteArrayContent(pdf);
            part.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(part, "file", "debug.pdf");

            var response = await _http.PostAsync("api/documents/extract/debug", content, ct);
            await response.EnsureSuccessWithMessageAsync();
            return (await response.Content.ReadFromJsonAsync<List<ExtractionDebugPage>>(cancellationToken: ct))!;
        }
    }
}
