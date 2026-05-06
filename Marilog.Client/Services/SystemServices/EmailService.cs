using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
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

        public async Task<IReadOnlyList<EmailResponse>> GetByEntityAsync(string entityType, int entityId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<EmailResponse>>>($"{Base}/by-entity?entityType={Uri.EscapeDataString(entityType)}&entityId={entityId}", ct);
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

        public async Task<EmailResponse> CreateAsync(string entityType, int entityId, string subject, string body,
            EmailDirection direction, IReadOnlyList<EmailParticipantResponse> participants,
            CancellationToken ct = default)
        {
            var request = new CreateEmailRequest
            {
                EntityType = entityType,
                EntityId = entityId,
                Subject = subject,
                Body = body,
                Direction = direction,
                Participants = participants
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<EmailResponse>>(ct);
            return response!.Data!;
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
            ParticipantType participantType, int participantId,
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

        // ── Attachments ───────────────────────────────────────────────────────────

        public async Task<EmailAttachmentResponse> AddAttachmentAsync(int emailId, string fileName,
            string filePath, long fileSizeBytes, CancellationToken ct = default)
        {
            var request = new AddAttachmentRequest { FileName = fileName, FilePath = filePath, FileSizeBytes = fileSizeBytes };
            var http = await _http.PostAsJsonAsync($"{Base}/{emailId}/attachments", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<EmailAttachmentResponse>>(ct);
            return response!.Data!;
        }

        public async Task RemoveAttachmentAsync(int emailId, int attachmentId, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{emailId}/attachments/{attachmentId}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}