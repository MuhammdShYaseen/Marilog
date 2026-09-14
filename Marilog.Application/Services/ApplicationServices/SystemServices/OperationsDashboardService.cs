using Marilog.Application.Interfaces.Services;
using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Reports.VoyageReports;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;


namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class OperationsDashboardService : IOperationsDashboardService
    {
        private readonly IVoyageService _voyageService;
        private readonly ICrewContractService _crewService;
        private readonly IDocumentService _documentService;

        public OperationsDashboardService(IVoyageService voyageService, ICrewContractService crewService, IDocumentService documentService)
        {
            _voyageService = voyageService;
            _crewService = crewService;
            _documentService = documentService;
        }

        public async Task<OperationsDashboardResponse> GetAsync(CancellationToken ct = default)
        {
            // Sequential execution

            var activeVoyages =
                await _voyageService.GetActiveVoyagesAsync(ct)
                ?? new List<VoyageResponse>();

            var expiringContracts =
                await _crewService.GetAboutExpireAsync(ct)
                ?? new List<CrewContractResponse>();

            var expiredContracts =
                await _crewService.GetExpiredAsync(ct)
                ?? new List<CrewContractResponse>();

            var unpaidDocuments =
                await _documentService.GetUnpaidAsync(false, ct)
                ?? new List<DocumentResponse>();

            // Date logic

            var today = DateTime.Now.Date;

            var next7Days = today.AddDays(7);

            // Upcoming Arrivals

            var upcoming = activeVoyages
                .Where(v =>
                    v.ArrivalDate.HasValue &&
                    v.ArrivalDate.Value.Date >= today &&
                    v.ArrivalDate.Value.Date <= next7Days)
                .OrderBy(v => v.ArrivalDate)
                .ToList();

            // Aggregations

            var totalCash = activeVoyages.Sum(v => v.CashOnBoard);

            var totalCargo = activeVoyages.Sum(v => v.CargoQuantityMT ?? 0);

            // Alerts

            var alerts = new List<string>();

            if (expiringContracts.Count > 0)
            {
                alerts.Add($"{expiringContracts.Count} crew contracts expiring soon");
            }

            if (expiredContracts.Count > 0)
            {
                alerts.Add($"{expiredContracts.Count} crew contracts expired");
            }

            if (unpaidDocuments.Count > 0)
            {
                alerts.Add($"{unpaidDocuments.Count} unpaid documents");
            }

            if (upcoming.Any())
            {
                alerts.Add($"{upcoming.Count} vessels arriving within 7 days");
            }

            return new OperationsDashboardResponse
            {
                ActiveVoyages = activeVoyages,
                UpcomingArrivals = upcoming,
                TotalCashOnBoard = totalCash,
                TotalCargoMT = totalCargo,
                ExpiringCrewContracts = expiringContracts,
                ExpiredCrewContracts = expiredContracts,
                Documents = unpaidDocuments,
                Alerts = alerts
            };
        }

        public async Task<IReadOnlyList<FinancialChartPointResponse>> GetFinancialSummaryAsync(DocumentFilterOptions options, CancellationToken ct = default)
        {
            var report = await _documentService.GetFilteredDocsReportAsync(options, ct);

            return report.MonthlySummary
                .Select(m =>
                {
                    var periodStart = new DateTime(m.Year, m.Month, 1);
                    return new FinancialChartPointResponse
                    {
                        PeriodStart = periodStart,
                        PeriodLabel = periodStart.ToString("MMM yyyy"),
                        Revenue = m.Revenue,
                        Expense = m.Expense,
                        CurrencyCode = report.BaseCurrencyCode
                    };
                })
                .ToList();
        }

        public async Task<IReadOnlyList<VoyageChartPointResponse>> GetVoyageSummaryAsync(DateOnly from, DateOnly to, int? vesselId = null, CancellationToken ct = default)
        {
            var report = await _voyageService.GetVoyagesReportAsync(
                new VoyageReportFilterOptions
                {
                    FromDate = from.ToDateTime(TimeOnly.MinValue),
                    ToDate = to.ToDateTime(TimeOnly.MaxValue),
                    VesselId = vesselId
                },
                ct);

            return report.Voyages
                .Where(v => v.DepartureDate.HasValue)
                .GroupBy(v => new { v.DepartureDate!.Value.Year, v.DepartureDate.Value.Month })
                .Select(g =>
                {
                    var periodStart = new DateTime(g.Key.Year, g.Key.Month, 1);
                    return new VoyageChartPointResponse
                    {
                        PeriodStart = periodStart,
                        PeriodLabel = periodStart.ToString("MMM yyyy"),
                        VoyageCount = g.Count(),
                        CargoMT = g.Sum(v => v.CargoQuantityMT ?? 0)
                    };
                })
                .OrderBy(p => p.PeriodStart)
                .ToList();
        }
    }
}
    