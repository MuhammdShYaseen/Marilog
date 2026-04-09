

namespace Marilog.Application.DTOs.Responses
{
    public class OperationsDashboardResponse
    {
        public IReadOnlyList<VoyageResponse> ActiveVoyages { get; set; } = new List<VoyageResponse>();
        

        public List<VoyageResponse> UpcomingArrivals { get; set; }
            = new();

        public decimal TotalCashOnBoard { get; set; }

        public decimal TotalCargoMT { get; set; }

        public IReadOnlyList<CrewContractResponse> ExpiringCrewContracts { get; set; } = new List<CrewContractResponse>();
        public IReadOnlyList<CrewContractResponse> ExpiredCrewContracts { get; set; } = new List<CrewContractResponse>();

        public IReadOnlyList<DocumentResponse> Documents { get; set; } = new List<DocumentResponse>();

        public List<string> Alerts { get; set; }
            = new();
    }
}
