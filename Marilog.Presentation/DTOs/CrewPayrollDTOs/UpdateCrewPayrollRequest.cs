namespace Marilog.Presentation.DTOs.CrewPayrollDTOs
{
    public class UpdateCrewPayrollRequest
    {
        public int WorkingDays { get; set; }
        public decimal BasicWage { get; set; }
        public decimal Allowances { get; set; } = 0m;
        public decimal Deductions { get; set; } = 0m;
        public string? Notes { get; set; }
    }
}
