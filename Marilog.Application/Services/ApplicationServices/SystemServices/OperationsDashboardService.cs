using Marilog.Application.Interfaces.Services;
using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Reports.VoyageReports;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;


namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class OperationsDashboardService : IOperationsDashboardService
    {
        private readonly IVoyageService _voyageService;
        private readonly ICrewContractService _crewService;
        private readonly IDocumentService _documentService;
        private readonly ICurrencyService _currencyService;

        public OperationsDashboardService(IVoyageService voyageService, ICrewContractService crewService,
            IDocumentService documentService, ICurrencyService currencyService)
        {
            _voyageService = voyageService;
            _crewService = crewService;
            _documentService = documentService;
            _currencyService = currencyService;
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

            var unpaidDocuments = await _documentService.GetUnpaidAsync(false, ct);
            if (unpaidDocuments == null)
                unpaidDocuments = new List<DocumentResponse>();
            unpaidDocuments = unpaidDocuments.Where(d => d.Side != FinancialSide.None).ToList();

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


        public async Task<IReadOnlyList<CurrencyTickerItemResponse>> GetCurrencyTickerAsync(CancellationToken ct = default)
        {
            var baseCurrency = await _currencyService.GetBaseCurrencyAsync(ct)
                ?? throw new InvalidOperationException("Base currency is not set.");

            var activeCurrencies = await _currencyService.GetActiveAsync(ct);

            var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            var latest = await _currencyService.GetLatestRatesAsync(ct);
            var previous = await _currencyService.GetHistoricalRatesAsync(yesterday, ct);

            var baseNow = ToUsdRate(latest, baseCurrency.Code);
            var basePrev = ToUsdRate(previous, baseCurrency.Code);
            if (baseNow is null)
                return [];

            var items = new List<CurrencyTickerItemResponse>();

            foreach (var currency in activeCurrencies.Where(c => c.Code != baseCurrency.Code))
            {
                var now = ToUsdRate(latest, currency.Code);
                if (now is null)
                    continue;

                var rateInBase = now.Value / baseNow.Value;

                var item = new CurrencyTickerItemResponse
                {
                    Code = currency.Code,
                    RateInBase = Math.Round(rateInBase, 8),
                    Trend = RateTrend.Unknown,
                    BaseCurrencyCode = baseCurrency.Code
                };

                var prev = ToUsdRate(previous, currency.Code);
                if (prev is not null && basePrev is not null)
                {
                    var prevInBase = prev.Value / basePrev.Value;
                    var change = Math.Round((rateInBase - prevInBase) / prevInBase * 100m, 2);

                    item.PreviousRateInBase = Math.Round(prevInBase, 8);
                    item.ChangePercent = change;
                    item.Trend = change > 0 ? RateTrend.Up
                               : change < 0 ? RateTrend.Down
                               : RateTrend.Flat;
                }

                items.Add(item);
            }

            return items.OrderBy(i => i.Code).ToList();
        }

        // USD is the pivot: the provider never returns it, its rate is always 1.
        private static decimal? ToUsdRate(IReadOnlyDictionary<string, decimal> rates, string code)
        {
            if (code == Currency.PivotCurrencyCode)
                return 1m;

            return rates.TryGetValue(code, out var rate) ? rate : null;
        }
    }
}
    