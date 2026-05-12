using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.Contract;
using Marilog.Contracts.DTOs.Requests.ContractDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Primitives;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class ContractService : IContractService
    {
        private readonly HttpClient _http;
        private const string Base = "api/contracts";

        public ContractService(HttpClient http) => _http = http;

        // ─── Queries ───────────────────────────────────────────────────────

        public async Task<ContractDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<ContractDetailResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<ContractDetailResponse?> GetByNumberAsync(string number, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<ContractDetailResponse>>($"{Base}/by-number/{Uri.EscapeDataString(number)}", ct);
            return response?.Data;
        }

        public async Task<PagedResponse<ContractSummary>> GetPagedAsync(ContractFilter filter, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/paged", filter, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ContractSummary>>>(ct);
            return response!.Data!;
        }

        public async Task<List<ContractSummary>> GetExpiringAsync(int withinDays, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<ContractSummary>>>($"{Base}/expiring?withinDays={withinDays}", ct);
            return response?.Data ?? [];
        }

        public async Task<ContractReport> GetReportAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<ContractReport>>($"{Base}/report", ct);
            return response!.Data!;
        }

        // ─── Write ────────────────────────────────────────────────────────

        public async Task<Result> CreateAsync(string contractNumber, string type, DateOnly effectiveDate, DateOnly? expiryDate, string? notes, CancellationToken ct = default)
        {
            var request = new CreateContractRequest
            (
                contractNumber,
                type,
                effectiveDate,
                expiryDate,
                notes
            );

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> UpdateAsync(int id, string contractNumber, string type, DateOnly effectiveDate, DateOnly? expiryDate, string? notes, CancellationToken ct = default)
        {
            var request = new UpdateContractRequest
            (
                id,
                contractNumber,
                type,
                effectiveDate,
                expiryDate,
                notes
            );
            var http = await _http.PutAsJsonAsync($"{Base}/{id}/update",request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }
        public async Task<Result> ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/activate", null, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> SuspendAsync(int id, string reason, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/suspend", reason, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> TerminateAsync(int id, string reason, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/terminate", reason, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> MarkExpiredAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/mark-expired", null, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> AddPartyAsync(int id, int companyId, string role, CancellationToken ct = default)
        {
            var request = new PartyRequest (companyId, role );
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/party", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> RemovePartyAsync(int id, int companyId, string role, CancellationToken ct = default)
        {
            var request = new PartyRequest (companyId, role );
            var http = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"{Base}/{id}/party")
            {
                Content = JsonContent.Create(request)
            }, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> RemovePartyViaAmendmentAsync(int id, int companyId, string role, int amendmentNumber, CancellationToken ct = default)
        {
            var request = new RemovePartyAmendmentRequest (companyId, role, amendmentNumber);
            var http = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"{Base}/{id}/party/amendment")
            {
                Content = JsonContent.Create(request)
            }, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> RecordAmendmentAsync(int id, string description, DateOnly effectiveDate, string changedBy, CancellationToken ct = default)
        {
            var request = new AmendmentRequest (description,  effectiveDate, changedBy );
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/amendment", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> ExtendExpiryAsync(int id, DateOnly newExpiryDate, int amendmentNumber, CancellationToken ct = default)
        {
            var request = new ExtendExpiryRequest (newExpiryDate, amendmentNumber);
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/extend-expiry", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> AttachFileAsync(int id, string fileUrl, string fileName, CancellationToken ct = default)
        {
            var request = new AttachFileRequest (fileUrl, fileName);
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/attach-file", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }
    }
}