using Marilog.Contracts.Common;
using Marilog.Contracts.Interfaces.Services.SystemServices;

namespace Marilog.Client.Services.SystemServices
{
    public class ShipUnlockService : IShipUnlockService
    {
        private readonly HttpClient _http;
        private const string Base = "api/ship-unlock";

        public ShipUnlockService(HttpClient http) => _http = http;

        public async Task<string> GetShipKeyAsync(string imoNumber, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<string>>(
                $"{Base}/key?imo={Uri.EscapeDataString(imoNumber)}", ct);
            return response?.Data ?? string.Empty;
        }

        public async Task<string> GetUnlockCodeAsync(string imoNumber, string requestCode, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<string>>(
                $"{Base}/code?imo={Uri.EscapeDataString(imoNumber)}&request={Uri.EscapeDataString(requestCode)}", ct);
            return response?.Data ?? string.Empty;
        }
    }
}