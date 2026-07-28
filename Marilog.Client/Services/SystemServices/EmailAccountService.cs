using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Marilog.Client.Services.SystemServices
{
    public class EmailAccountService : IEmailAccountService
    {
        private readonly HttpClient _http;
        private const string BaseRoute = "api/email-accounts";

        public EmailAccountService(HttpClient http) => _http = http;

        public async Task<EmailAccountResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"{BaseRoute}/{id}", ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<EmailAccountResponse>(cancellationToken: ct);
        }

        public async Task<IReadOnlyList<EmailAccountResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<EmailAccountResponse>>(BaseRoute, ct);
            return result ?? new List<EmailAccountResponse>();
        }

        public async Task<EmailAccountResponse> CreateAsync(CreateEmailAccountRequest request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync(BaseRoute, request, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<EmailAccountResponse>(cancellationToken: ct))!;
        }

        public async Task RenameAsync(int id, string displayName, CancellationToken ct = default)
        {
            var request = new RenameEmailAccountRequest { DisplayName = displayName };
            var response = await _http.PatchAsJsonAsync($"{BaseRoute}/{id}/rename", request, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateConfigAsync(int id, Dictionary<string, string>? config, CancellationToken ct = default)
        {
            var request = new UpdateEmailAccountConfigRequest { Config = config };
            var response = await _http.PatchAsJsonAsync($"{BaseRoute}/{id}/config", request, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.PatchAsync($"{BaseRoute}/{id}/activate", null, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.PatchAsync($"{BaseRoute}/{id}/deactivate", null, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"{BaseRoute}/{id}", ct);
            response.EnsureSuccessStatusCode();
        }

        public Task<Dictionary<string, string>> GetDecryptedConfigAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task MarkSyncedAsync(int id, DateTime syncedAt, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
