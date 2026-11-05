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
            var response = await _http.GetCheckedAsync<ApiResponse<ContractDetailResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<ContractDetailResponse?> GetByNumberAsync(string number, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<ContractDetailResponse>>($"{Base}/by-number/{Uri.EscapeDataString(number)}", ct);
            return response?.Data;
        }

        public async Task<PagedResponse<ContractSummary>> GetPagedAsync(ContractFilter filter, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/paged", filter, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ContractSummary>>>(ct);
            return response!.Data!;
        }

        public async Task<List<ContractSummary>> GetExpiringAsync(int withinDays, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<List<ContractSummary>>>($"{Base}/expiring?withinDays={withinDays}", ct);
            return response?.Data ?? [];
        }

        public async Task<ContractReport> GetReportAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<ContractReport>>($"{Base}/report", ct);
            return response!.Data!;
        }

        // ─── Write ────────────────────────────────────────────────────────

        public async Task<Result> CreateAsync(string contractNumber, int contractTypeId, DateOnly effectiveDate, DateOnly? expiryDate, string? notes, CancellationToken ct = default)
        {
            var request = new CreateContractRequest
            (
                contractNumber,
                contractTypeId,
                effectiveDate,
                expiryDate,
                notes
            );

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> UpdateAsync(int id, string contractNumber, int contractTypeId, DateOnly effectiveDate, DateOnly? expiryDate, string? notes, CancellationToken ct = default)
        {
            var request = new UpdateContractRequest
            (
                id,
                contractNumber,
                contractTypeId,
                effectiveDate,
                expiryDate,
                notes
            );
            var http = await _http.PutAsJsonAsync($"{Base}/{id}/update", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }
        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/activate", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> SuspendAsync(int id, string reason, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/suspend", reason, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> TerminateAsync(int id, string reason, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/terminate", reason, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> MarkExpiredAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/mark-expired", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> AddPartyAsync(int id, int companyId, int roleId, CancellationToken ct = default)
        {
            var request = new PartyRequest(companyId, roleId);
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/party", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> RemovePartyAsync(int id, int companyId, int roleId, CancellationToken ct = default)
        {
            var request = new PartyRequest(companyId, roleId);
            var http = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"{Base}/{id}/party")
            {
                Content = JsonContent.Create(request)
            }, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }


        public async Task<Result> RecordAmendmentAsync(int id, string description, DateOnly effectiveDate, CancellationToken ct = default)
        {
            var request = new AmendmentRequest(description, effectiveDate);
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/amendment", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

        public async Task<Result> ChangeExpiryAsync(int id, DateOnly newExpiryDate, CancellationToken ct = default)
        {
            var request = new ChangeExpiryRequest(newExpiryDate);
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/change-expiry", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<Result>>(ct);
            return response!.Data!;
        }

    }
}