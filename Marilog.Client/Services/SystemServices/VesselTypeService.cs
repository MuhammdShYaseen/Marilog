using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.VesselTypeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class VesselTypeService : IVesselTypeService
    {
        private readonly HttpClient _http;
        private const string Base = "api/vessel-types";

        public VesselTypeService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<VesselTypeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<VesselTypeResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<VesselTypeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<VesselTypeResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public async Task<VesselTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<VesselTypeResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<VesselTypeResponse> CreateAsync(CreateVesselTypeRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<VesselTypeResponse>>(ct);
            return response!.Data!;
        }
        public async Task<IReadOnlyList<VesselTypeResponse>> CreateRangeAsync(
            IEnumerable<CreateVesselTypeRequest> requests, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", requests, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<VesselTypeResponse>>>(ct);
            return response?.Data ?? [];
        }
        public async Task UpdateAsync(int id, UpdateVesselTypeRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/activate", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/deactivate", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }
    }
}