namespace Marilog.Application.DTOs.Reports.CrewPayrollReports
{
    public class VesselPayrollSummary
    {
        public int VesselId { get; set; }
        public string? VesselName { get; set; } = "";
        public decimal TotalGross { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal TotalRemaining { get; set; }
        public int ContractsCount { get; set; }
    }
}