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

        public async Task<OperationsDashboardResponse> GetAsync(CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var next7Days = today.AddDays(7);

            // Active Voyages

            var activeVoyages = await _voyageService.GetActiveVoyagesAsync(ct);

            // Upcoming Arrivals

            var upcoming = activeVoyages
            .Where(v =>
                v.ArrivalDate.HasValue &&
                v.ArrivalDate.Value.Date >= today &&
                v.ArrivalDate.Value.Date <= next7Days)
            .ToList();

            // Aggregations

            var totalCash = activeVoyages.Sum(v => v.CashOnBoard);

            var totalCargo = activeVoyages.Sum(v =>  v.CargoQuantityMT ?? 0);

            // Crew Contracts

            var expiringContracts =  await _crewService.GetAboutExpireAsync(ct);
            var expairedContracts = await _crewService.GetExpiredAsync(ct);
            var unpaidDocuments = await _documentService.GetUnpaidAsync(ct);

            // Alerts

            var alerts = new List<string>();

            if (expiringContracts.Count > 0)
            {
                alerts.Add(
                    $"{expiringContracts.Count} crew contracts about Expaird");
            }

            if (expairedContracts.Count > 0) 
            {
                alerts.Add(
                    $"{expairedContracts.Count} crew contracts Expaird");
            }

            if (unpaidDocuments.Count > 0)
            {
                alerts.Add(
                    $"{unpaidDocuments.Count} unpaid document");
            }

            if (upcoming.Any())
            {
                alerts.Add(
                    $"{upcoming.Count} vessels arriving soon");
            }

            return new OperationsDashboardResponse
            {
                ActiveVoyages = activeVoyages,
                UpcomingArrivals = upcoming,
                TotalCashOnBoard = totalCash,
                TotalCargoMT = totalCargo,
                ExpiringCrewContracts = expiringContracts,
                Alerts = alerts,
                Documents = unpaidDocuments,
                ExpiredCrewContracts = expairedContracts,
            };
        }
    }
}
