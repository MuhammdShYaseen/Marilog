using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.EmailServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _http;
        private const string Base = "api/emails";

        public EmailService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<EmailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<EmailResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<EmailResponse?> GetFullAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<EmailResponse>>($"{Base}/{id}/full", ct);
            return response?.Data;
        }
        public async Task<PagedResponse<EmailResponse>> GetInboxAsync(PagedRequest request, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<PagedResponse<EmailResponse>>(
                $"{Base}/inbox?page={request.Page}&pageSize={request.PageSize}", ct)
                ?? new PagedResponse<EmailResponse>();
        }

        public async Task<PagedResponse<EmailResponse>> GetOutboxAsync(PagedRequest request, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<PagedResponse<EmailResponse>>(
                $"{Base}/outbox?page={request.Page}&pageSize={request.PageSize}", ct)
                ?? new PagedResponse<EmailResponse>();
        }
        public async Task<IReadOnlyList<EmailResponse>> GetByEntityAsync(EntityType entityType, int entityId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<EmailResponse>>>($"{Base}/by-entity?entityType={entityType}&entityId={entityId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<EmailResponse>> GetByStatusAsync(EmailStatus status, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<EmailResponse>>>($"{Base}/by-status/{status}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<EmailResponse>> GetByParticipantAsync(ParticipantType participantType, int participantId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<EmailResponse>>>($"{Base}/by-participant?participantType={participantType}&participantId={participantId}", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<EmailResponse> CreateAsync(CreateEmailRequest request,
            CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<EmailResponse>>(ct);
            return response!.Data!;
        }

        public async Task<EmailResponse> RelinkAsync(int emailId, EntityType entityType, int entityId, CancellationToken ct = default)
        {
            var request = new UpsertEmailEntityRequest
            {
                EntityType = entityType,
                EntityId = entityId
            };

            var response = await _http.PutAsJsonAsync($"api/emails/{emailId}/entity", request, ct);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<EmailResponse>(cancellationToken: ct))!;
        }

        public async Task MarkAsSentAsync(int id, DateTime sentAt, string? externalId = null, CancellationToken ct = default)
        {
            var request = new MarkEmailSentRequest { SentAt = sentAt, ExternalId = externalId };
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/mark-sent", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task MarkAsReceivedAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/mark-received", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task MarkAsFailedAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/mark-failed", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RetryAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/retry", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Participants ──────────────────────────────────────────────────────────

        public async Task<EmailParticipantResponse> AddParticipantAsync(int emailId, ParticipantRole role,
            ParticipantType participantType, int? participantId,
            string? displayName = null, string? emailAddress = null,
            CancellationToken ct = default)
        {
            var request = new AddParticipantRequest
            {
                Role = role,
                ParticipantType = participantType,
                ParticipantId = participantId,
                DisplayName = displayName,
                EmailAddress = emailAddress
            };

            var http = await _http.PostAsJsonAsync($"{Base}/{emailId}/participants", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<EmailParticipantResponse>>(ct);
            return response!.Data!;
        }

        public async Task RemoveParticipantAsync(int emailId, int participantId, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{emailId}/participants/{participantId}", ct);
            http.EnsureSuccessStatusCode();
        }

        public Task<IReadOnlyList<EmailResponse>> GetUnlinkedAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<EmailResponse> CreateFromInboundAsync(int accountId, InboundMessage message, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<EmailResponse> SendEmailAsync(int emailId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<EmailResponse> CreateFromSentAsync(int accountId, InboundMessage message, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}