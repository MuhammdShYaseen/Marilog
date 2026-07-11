namespace Marilog.OCR.Worker.Services
{
    // CallBackService.cs
    public sealed class CallBackService : ICallBackService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CallBackService> _logger;

        public CallBackService(HttpClient httpClient, ILogger<CallBackService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task NotifyOcrCompletedAsync(Guid documentId, string extractedContent, string? thumbnailPath, CancellationToken ct = default)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/StoredFiles/{documentId}/ocr-content", new { Content = extractedContent, thumbnailPath },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "Callback failed | DocumentId: {Id} | Status: {Status} | Error: {Error}",
                    documentId, response.StatusCode, error);

                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation("Callback succeeded | DocumentId: {Id}", documentId);
        }
    }
}
