namespace Marilog.Application.DTOs.Reports.CrewPayrollReports
{
    public class MonthlyPayrollSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalGross { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal TotalRemaining { get; set; }
    }
}