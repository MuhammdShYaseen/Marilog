using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.ContractLookupDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class ContractTypeService : IContractTypeService
    {
        private readonly HttpClient _http;
        private const string Base = "api/contract-types";

        public ContractTypeService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<ContractLookupResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<ContractLookupResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<ContractLookupResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<ContractLookupResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public async Task<ContractLookupResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<ContractLookupResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<ContractLookupResponse> CreateAsync(string code, string name, int sortOrder = 0, CancellationToken ct = default)
        {
            var request = new CreateContractLookupRequest { Code = code, Name = name, SortOrder = sortOrder };
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<ContractLookupResponse>>(ct);
            return response!.Data!;
        }

        public async Task UpdateAsync(int id, string name, int sortOrder, CancellationToken ct = default)
        {
            var request = new UpdateContractLookupRequest { Name = name, SortOrder = sortOrder };
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
