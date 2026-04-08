

namespace Marilog.Application.DTOs.Responses
{
    public class OperationsDashboardResponse
    {
        public List<VoyageResponse> ActiveVoyages { get; set; } = new();

        public int ExpiringCrewContracts { get; set; }

        public int PendingCrewContracts { get; set; }

        public List<ActivityDto> UpcomingActivities { get; set; } = new();

        public List<AlertDto> Alerts { get; set; } = new();
    }
}
