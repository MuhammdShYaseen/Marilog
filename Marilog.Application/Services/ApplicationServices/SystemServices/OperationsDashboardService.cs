using Marilog.Application.Interfaces.Services;
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
    }
}
