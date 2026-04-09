using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;


namespace Marilog.Application.Services.ApplicationServices
{
    public class OperationsDashboardService
    : IOperationsDashboardService
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

        public async Task<OperationsDashboardResponse>GetAsync(CancellationToken ct = default)
        {
            // ─────────────────────────────────────────────
            // Parallel execution
            // ─────────────────────────────────────────────

            var activeVoyagesTask = _voyageService.GetActiveVoyagesAsync(ct);

            var expiringContractsTask = _crewService.GetAboutExpireAsync(ct);

            var expiredContractsTask = _crewService.GetExpiredAsync(ct);

            var unpaidDocumentsTask = _documentService.GetUnpaidAsync(ct);

            await Task.WhenAll(activeVoyagesTask, expiringContractsTask, expiredContractsTask, unpaidDocumentsTask);

            // ─────────────────────────────────────────────
            // Await results safely
            // ─────────────────────────────────────────────

            var activeVoyages = await activeVoyagesTask ?? new List<VoyageResponse>();

            var expiringContracts = await expiringContractsTask ?? new List<CrewContractResponse>();

            var expiredContracts = await expiredContractsTask ?? new List<CrewContractResponse>();

            var unpaidDocuments = await unpaidDocumentsTask ?? new List<DocumentResponse>();

            // ─────────────────────────────────────────────
            // Date logic
            // ─────────────────────────────────────────────

            var today = DateTime.Now.Date;

            var next7Days = today.AddDays(7);

            // ─────────────────────────────────────────────
            // Upcoming Arrivals
            // ─────────────────────────────────────────────

            var upcoming = activeVoyages
                    .Where(v =>
                        v.ArrivalDate.HasValue &&
                        v.ArrivalDate.Value.Date >= today &&
                        v.ArrivalDate.Value.Date <= next7Days)
                    .OrderBy(v => v.ArrivalDate)
                    .ToList();

            // ─────────────────────────────────────────────
            // Aggregations
            // ─────────────────────────────────────────────

            var totalCash = activeVoyages.Sum(v => v.CashOnBoard);

            var totalCargo = activeVoyages.Sum(v => v.CargoQuantityMT ?? 0);

            // ─────────────────────────────────────────────
            // Alerts
            // ─────────────────────────────────────────────

            var alerts = new List<string>();

            if (expiringContracts.Count > 0)
            {
                alerts.Add(
                    $"{expiringContracts.Count} crew contracts expiring soon");
            }

            if (expiredContracts.Count > 0)
            {
                alerts.Add(
                    $"{expiredContracts.Count} crew contracts expired");
            }

            if (unpaidDocuments.Count > 0)
            {
                alerts.Add($"{unpaidDocuments.Count} unpaid documents");
            }

            if (upcoming.Any())
            {
                alerts.Add( $"{upcoming.Count} vessels arriving within 7 days");
            }

            // ─────────────────────────────────────────────
            // Final Response
            // ─────────────────────────────────────────────

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
    }
}
