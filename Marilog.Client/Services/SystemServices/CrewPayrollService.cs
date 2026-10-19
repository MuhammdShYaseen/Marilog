using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.CrewPayrollReports;
using Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class CrewPayrollService : ICrewPayrollService
    {
        private readonly HttpClient _http;
        private const string Base = "api/crew-payrolls";

        public CrewPayrollService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CrewPayrollResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<CrewPayrollResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<CrewPayrollResponse?> GetWithDisbursementsAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<CrewPayrollResponse>>($"{Base}/{id}/with-disbursements", ct);
            return response?.Data;
        }

        public async Task<CrewPayrollResponse?> GetByContractAndMonthAsync(int contractId, DateOnly month, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<CrewPayrollResponse>>(
                $"{Base}/by-contract/{contractId}/month?month={month:yyyy-MM-dd}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> GetByContractAsync(int contractId, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<CrewPayrollResponse>>>(
                $"{Base}/by-contract/{contractId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> GetByMonthAsync(DateOnly month, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<CrewPayrollResponse>>>(
                $"{Base}/by-month?month={month:yyyy-MM-dd}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> GetByStatusAsync(PayrollStatus status, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<CrewPayrollResponse>>>(
                $"{Base}/by-status/{status}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> GetOutstandingAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<CrewPayrollResponse>>>(
                $"{Base}/outstanding", ct);
            return response?.Data ?? [];
        }

        // ── Commands ──────────────────────────────────────────────────────────────

        public async Task<CrewPayrollResponse> CreateAsync(int contractId, DateOnly payrollMonth,
     int currencyId, decimal allowances = 0m, decimal deductions = 0m,
     string? notes = null, CancellationToken ct = default)
        {
            var request = new CreateCrewPayrollRequest
            {
                ContractId = contractId,
                PayrollMonth = payrollMonth,
                CurrencyId = currencyId,
                Allowances = allowances,
                Deductions = deductions,
                Notes = notes
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<CrewPayrollResponse>>(ct);
            return response!.Data!;
        }

        public async Task UpdateAsync(int id, int contractId, int currencyId,
            decimal allowances = 0m, decimal deductions = 0m, string? notes = null,
            CancellationToken ct = default)
        {
            var request = new UpdateCrewPayrollRequest
            {
                ContractId = contractId,
                CurrencyId = currencyId,
                Allowances = allowances,
                Deductions = deductions,
                Notes = notes
            };

            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> CreateRangeAsync(
            IEnumerable<CreateCrewPayrollRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CrewPayrollResponse>>>(ct);
            return response?.Data ?? [];
        }

        

        public async Task ApproveAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/approve", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task CancelAsync(int id, string reason, CancellationToken ct = default)
        {
            var request = new CancelPayrollRequest { Reason = reason };
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/cancel", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        // ── Disbursements ─────────────────────────────────────────────────────────

        public async Task<DisbursementResponse> PayAsync(int payrollId, PayCrewPayrollRequest request,
            CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{payrollId}/disbursements", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<DisbursementResponse>>(ct);
            return response!.Data!;
        }

        public async Task ConfirmDisbursementAsync(int payrollId, int disbursementId, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{payrollId}/disbursements/{disbursementId}/confirm", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task CancelDisbursementAsync(int payrollId, int disbursementId, string reason,
            CancellationToken ct = default)
        {
            var request = new CancelDisbursementRequest { Reason = reason };
            var http = await _http.PatchAsJsonAsync(
                $"{Base}/{payrollId}/disbursements/{disbursementId}/cancel", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        // ── Reports ───────────────────────────────────────────────────────────────

        public Task<CrewPayrollReport> GetCrewPayrollReportAsync(CrewPayrollFilterOptions options, CancellationToken ct = default)
            => throw new NotImplementedException("Endpoint not yet defined on the backend.");
    }
}