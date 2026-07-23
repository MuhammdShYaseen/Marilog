using Marilog.Contracts.Interfaces.DataManagment;
using System.Net.Http.Headers;

namespace Marilog.Client.Services.DataManagment
{
    public sealed class FullBackupService : IFullBackupService
    {
        private readonly HttpClient _httpClient;

        public string FileExtension => ".zip";
        private const string Base = "api/backup";
        public FullBackupService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateBackupAsync(Stream destination, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(destination);

            using var response = await _httpClient.GetAsync($"{Base}/create", HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            await using var responseStream =  await response.Content.ReadAsStreamAsync(ct);

            await responseStream.CopyToAsync(destination, ct);
        }

        public async Task RestoreBackupAsync(Stream source, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            using var streamContent = new StreamContent(source);

            streamContent.Headers.ContentType =  new MediaTypeHeaderValue("application/octet-stream");

            using var multipartContent =  new MultipartFormDataContent();

            multipartContent.Add(streamContent, "file", $"marilog-full-backup{FileExtension}");

            using var response = await _httpClient.PostAsync($"{Base}/backup/restore", multipartContent, ct);

            response.EnsureSuccessStatusCode();
        }
    }
}
