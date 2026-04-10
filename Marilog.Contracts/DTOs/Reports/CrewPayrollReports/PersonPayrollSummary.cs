namespace Marilog.Contracts.DTOs.Reports.CrewPayrollReports
{
    public class PersonPayrollSummary
    {
        public int PersonId { get; set; }
        public string? PersonFullName { get; set; } = "";
        public decimal TotalGross { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal TotalRemaining { get; set; }
        public int ContractsCount { get; set; }
    }
}