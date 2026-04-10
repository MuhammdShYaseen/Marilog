namespace Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs
{
    public class CreateCrewPayrollRequest
    {
        public int ContractId { get; set; }
        public DateOnly PayrollMonth { get; set; }
        public int WorkingDays { get; set; }
        public decimal BasicWage { get; set; }
        public decimal Allowances { get; set; } = 0m;
        public decimal Deductions { get; set; } = 0m;
        public string? Notes { get; set; }
    }
}
