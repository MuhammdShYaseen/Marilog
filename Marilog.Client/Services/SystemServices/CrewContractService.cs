using Marilog.Client.Extensions;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.CrewContractReports;
using Marilog.Contracts.DTOs.Requests.CrewDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Primitives;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Marilog.Client.Services.SystemServices
{
    public class CrewContractService : ICrewContractService
    {
        private readonly HttpClient _http;
        private const string Base = "api/crew-contracts";

        public CrewContractService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CrewContractResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CrewContractResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetByPersonAsync(int personId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<CrewContractResponse>>>($"{Base}/by-person/{personId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetByVesselAsync(int vesselId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<CrewContractResponse>>>($"{Base}/by-vessel/{vesselId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetActiveByVesselAsync(int vesselId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<CrewContractResponse>>>($"{Base}/active/by-vessel/{vesselId}", ct);
            return response?.Data ?? [];
        }

        public async Task<CrewContractResponse?> GetActiveMasterAsync(int vesselId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CrewContractResponse>>($"{Base}/active-master/{vesselId}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetActiveOnDateAsync(DateOnly date, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<IReadOnlyList<CrewContractResponse>>($"{Base}/active-on-date?date={date:yyyy-MM-dd}", ct);
            return response ?? [];
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetAllAsync(CancellationToken ct)
        {
            var response = await _http.GetFromJsonAsync <ApiResponse<IReadOnlyList <CrewContractResponse>>>($"{Base}/all", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CrewContractResponse> CreateAsync(int durationInMonth, int personId, int vesselId, int rankId,
            decimal monthlyWage, DateOnly signOnDate, int? signOnPort = null, string? notes = null,
            CancellationToken ct = default)
        {
            var request = new CreateCrewContractRequest
            {
                DurationInMonth = durationInMonth,
                PersonId = personId,
                VesselId = vesselId,
                RankId = rankId,
                MonthlyWage = monthlyWage,
                SignOnDate = signOnDate,
                SignOnPort = signOnPort,
                Notes = notes
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<CrewContractResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<CrewContractResponse>> CreateRangeAsync(IEnumerable<CreateCrewContractRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CrewContractResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task<Result> UpdateAsync(int id, int durationInMonth, int personId, int vesselId, int rankId,
                                       decimal monthlyWage, DateOnly signOnDate,
                                       int? signOnPort = null, string? notes = null,
                                       CancellationToken ct = default)
        {
            var request = new UpdateCrewContractRequest 
            {
                MonthlyWage = monthlyWage,
                Notes = notes,
                DurationInMonth = durationInMonth,
                PersonId = personId,
                VesselId = vesselId,
                RankId = rankId,
                SignOnDate = signOnDate,
                SignOnPort = signOnPort,
                Id = id
            };
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
            return Result.Ok();
        }

        public async Task<Result> SignOffAsync(int id, DateOnly signOffDate, int? signOffPort = null, CancellationToken ct = default)
        {
            var request = new SignOffCrewContractRequest { SignOffDate = signOffDate, SignOffPort = signOffPort };
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/sign-off", request, ct);
            http.EnsureSuccessStatusCode();
            return Result.Ok();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
            return Result.Ok();
        }

        // ── Reports ───────────────────────────────────────────────────────────────

        // No backend endpoints for these three — flag them when you add the controllers
        public Task<CrewContractReport> GetCrewContractsReportAsync(CrewContractFilterOptions options, CancellationToken ct = default)
            => throw new NotImplementedException("Endpoint not yet defined on the backend.");

        public Task<IReadOnlyList<CrewContractResponse>> GetExpiredAsync(CancellationToken ct)
            => throw new NotImplementedException("Endpoint not yet defined on the backend.");

        public Task<IReadOnlyList<CrewContractResponse>> GetAboutExpireAsync(CancellationToken ct = default)
        => _http.GetApiListAsync<CrewContractResponse>($"{Base}/about-expire", ct);
    }
}