using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.VoyageReports;
using Marilog.Contracts.DTOs.Requests.VoyageDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class VoyageService : IVoyageService
    {
        private readonly HttpClient _http;
        private const string Base = "api/voyages";

        public VoyageService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<VoyageResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // Controller returns unwrapped on this endpoint
            return await _http.GetFromJsonAsync<VoyageResponse>($"{Base}/{id}", ct);
        }

        public async Task<VoyageResponse?> GetWithStopsAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<VoyageResponse>>($"{Base}/{id}/with-stops", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<VoyageResponse>> GetByVesselAsync(int vesselId, CancellationToken ct = default)
        {
            // Controller returns unwrapped on this endpoint
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<VoyageResponse>>>($"{Base}/by-vessel/{vesselId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<VoyageResponse>> GetByMonthAsync(DateOnly month, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<VoyageResponse>>>($"{Base}/by-month?month={month:yyyy-MM-dd}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<VoyageResponse>> GetByStatusAsync(VoyageStatus status, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<VoyageResponse>>>($"{Base}/by-status?status={status}", ct);
            return response?.Data ?? [];
        }

        public async Task<VoyageResponse?> GetCurrentVoyageAsync(int vesselId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<VoyageResponse>>($"{Base}/current/{vesselId}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<VoyageResponse>> GetActiveVoyagesAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<VoyageResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public Task<VoyageReport> GetVoyagesReportAsync(VoyageReportFilterOptions options, CancellationToken ct = default)
            => throw new NotImplementedException("Endpoint in reports endpoint");

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<VoyageResponse> CreateAsync(int vesselId, string voyageNumber, DateOnly voyageMonth,
            int? masterContractId = null, int? departurePortId = null, int? arrivalPortId = null,
            DateTime? departureDate = null, DateTime? arrivalDate = null,
            string? cargoType = null, decimal? cargoQuantityMt = null, string? notes = null,
            CancellationToken ct = default)
        {
            var request = new CreateVoyageRequest(vesselId, voyageNumber, voyageMonth, masterContractId,
                departurePortId, arrivalPortId, departureDate, arrivalDate, cargoType, cargoQuantityMt, notes);

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<VoyageResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<VoyageResponse>> CreateRangeAsync(IEnumerable<CreateVoyageRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<VoyageResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, int? departurePortId, int? arrivalPortId,
            DateTime? departureDate, DateTime? arrivalDate,
            string? cargoType, decimal? cargoQuantityMt, string? notes,
            CancellationToken ct = default)
        {
            var request = new UpdateVoyageRequest
            (
                departurePortId,
                arrivalPortId,
                departureDate,
                arrivalDate,
                cargoType,
                cargoQuantityMt,
                notes
            );

            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UpdateFinancialsAsync(int id, decimal cashOnBoard,
            decimal cigarettesOnBoard, decimal previousMasterBalance, CancellationToken ct = default)
        {
            var request = new UpdateVoyageFinancialsRequest
            (
                cashOnBoard,
                cigarettesOnBoard,
                previousMasterBalance
            );

            var http = await _http.PutAsJsonAsync($"{Base}/{id}/financials", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task AssignMasterAsync(int id, int contractId, CancellationToken ct = default)
        {
            var request = new AssignVoyageMasterRequest (contractId );
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/assign-master", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task StartAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/start", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task CompleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/complete", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task CancelAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/cancel", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Stops ─────────────────────────────────────────────────────────────────

        public async Task<VoyageStopResponse> AddStopAsync(int voyageId, int portId, int stopOrder,
            DateTime? arrivalDate = null, DateTime? departureDate = null,
            string? purposeOfCall = null, string? notes = null, CancellationToken ct = default)
        {
            var request = new AddVoyageStopRequest
            (
                portId,
                stopOrder,
                arrivalDate,
                departureDate,
                purposeOfCall,
                notes
            );

            var http = await _http.PostAsJsonAsync($"{Base}/{voyageId}/stops", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<VoyageStopResponse>>(ct);
            return response!.Data!;
        }

        public async Task UpdateStopAsync(int voyageId, int stopOrder, DateTime? arrivalDate,
            DateTime? departureDate, string? purposeOfCall, string? notes, CancellationToken ct = default)
        {
            var request = new UpdateVoyageStopRequest
            (
                arrivalDate,
                departureDate,
                purposeOfCall,
                notes
            );

            var http = await _http.PutAsJsonAsync($"{Base}/{voyageId}/stops/{stopOrder}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RemoveStopAsync(int voyageId, int stopOrder, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{voyageId}/stops/{stopOrder}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}