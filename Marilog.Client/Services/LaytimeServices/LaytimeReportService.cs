using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;

namespace Marilog.Client.Services.LaytimeServices
{
    public sealed class LaytimeReportService : ILaytimeReportService
    {
        private readonly HttpClient _http;

        public LaytimeReportService(HttpClient http)
        {
            _http = http;
        }

        public async Task<byte[]> GenerateSummaryReportAsync(int calculationId, ReportFormat format, CancellationToken ct)
        {
            var url = $"api/laytime-reports/calculations/{calculationId}/summary?format={format}";

            return await _http.GetByteArrayAsync(url, ct);
        }

        public async Task<byte[]> GenerateDetailedReportAsync(int calculationId, ReportFormat format, CancellationToken ct)
        {
            var url = $"api/laytime-reports/calculations/{calculationId}/detailed?format={format}";

            return await _http.GetByteArrayAsync(url, ct);
        }

        public async Task<byte[]> GenerateDelayReportAsync(int calculationId, ReportFormat format,  CancellationToken ct)
        {
            var url = $"api/laytime-reports/calculations/{calculationId}/delays?format={format}";

            return await _http.GetByteArrayAsync(url, ct);
        }

        public async Task<byte[]> GenerateContractLaytimeReportAsync(int contractId, ReportFormat format, CancellationToken ct)
        {
            var url = $"api/laytime-reports/contracts/{contractId}/laytime?format={format}";

            return await _http.GetByteArrayAsync(url, ct);
        }

        public async Task<byte[]> GenerateTimeSheetExcelAsync(int calculationId, CancellationToken ct)
        {
            var url = $"api/laytime-reports/calculations/{calculationId}/timesheet-excel";

            return await _http.GetByteArrayAsync(url, ct);
        }
    }
}