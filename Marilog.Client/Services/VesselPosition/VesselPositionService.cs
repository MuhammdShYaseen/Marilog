

using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.VesselPositions;
using System.Net;
using System.Net.Http.Json;

namespace Marilog.Client.Services.VesselPosition
{
    public sealed class VesselPositionService(HttpClient http) : IVesselPositionService
    {
        public async Task<VesselPositionResponse?> GetByImoAsync(int imo, CancellationToken ct = default)
        {
            using var res = await http.GetAsync($"api/vessel-positions/{imo}", ct);
            if (res.StatusCode == HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<VesselPositionResponse>(ct);
        }
    }
}
