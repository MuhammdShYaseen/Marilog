using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.DataManagment;
using System.Net.Http.Json;

namespace Marilog.Client.Services.DataManagment
{
    public sealed class BackupJobService : IBackupJobService
    {
        private const string Base = "api/backup";
        private readonly HttpClient _http;

        public BackupJobService(HttpClient http) => _http = http;

        public async Task<BackupJobResponse> StartAsync(CancellationToken ct = default)
        {
            using var response = await _http.PostAsync($"{Base}/jobs", null, ct);
            await response.EnsureSuccessWithMessageAsync(ct);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<BackupJobResponse>>(cancellationToken: ct);
            return body?.Data ?? throw new InvalidOperationException("The server returned no backup status.");
        }

        public async Task<BackupJobResponse> GetAsync(string token, CancellationToken ct = default)
        {
            var body = await _http.GetCheckedAsync<ApiResponse<BackupJobResponse>>($"{Base}/jobs/{token}", ct);
            return body?.Data ?? throw new InvalidOperationException("The server returned no backup status.");
        }

        public string GetDownloadUrl(string token)
            => new Uri(_http.BaseAddress!, $"{Base}/download/{token}").ToString();
    }
}