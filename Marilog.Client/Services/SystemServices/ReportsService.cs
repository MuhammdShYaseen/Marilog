using Marilog.Client.Extensions;
using Marilog.Client.Interfaces;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.CrewContractReports;
using Marilog.Contracts.DTOs.Reports.CrewPayrollReports;
using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Reports.SwiftTransferReports;
using Marilog.Contracts.DTOs.Reports.VoyageReports;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class ReportsService : IReportsService
    {
        private readonly HttpClient _http;
        private const string Base = "api/reports";

        public ReportsService(HttpClient http) => _http = http;

        public async Task<SwiftTransferReport> GetSwiftTransfersReportAsync(SwiftTransferFilterOptions filter, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SwiftTransferReport>>($"{Base}/swift-transfers{filter.ToQueryString()}", ct);
            return response!.Data!;
        }

        public async Task<VoyageReport> GetVoyagesReportAsync(VoyageReportFilterOptions filter, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<VoyageReport>>($"{Base}/voyages{filter.ToQueryString()}", ct);
            return response!.Data!;
        }

        public async Task<CrewPayrollReport> GetCrewPayrollReportAsync(CrewPayrollFilterOptions filter, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CrewPayrollReport>>($"{Base}/crew-payroll{filter.ToQueryString()}", ct);
            return response!.Data!;
        }

        public async Task<CrewContractReport> GetCrewContractsReportAsync(CrewContractFilterOptions filter, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CrewContractReport>>($"{Base}/crew-contracts{filter.ToQueryString()}", ct);
            return response!.Data!;
        }

        public async Task<DocumentReport> GetDocumentsReportAsync(DocumentFilterOptions filter, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentReport>>($"{Base}/documents{filter.ToQueryString()}", ct);
            return response!.Data!;
        }
    }
}