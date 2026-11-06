using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CargoTypeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class CargoTypeService : ICargoTypeService
    {
        private readonly HttpClient _http;
        private const string Base = "api/cargo-types";

        public CargoTypeService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<CargoTypeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<CargoTypeResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<CargoTypeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<CargoTypeResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public async Task<CargoTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<CargoTypeResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<CargoTypeResponse> CreateAsync(CreateCargoTypeRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<CargoTypeResponse>>(ct);
            return response!.Data!;
        }

        public async Task UpdateAsync(int id, UpdateCargoTypeRequest request, CancellationToken ct = default)
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