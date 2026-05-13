namespace Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs
{
    public class UpdateCrewPayrollRequest
    {
        public int ContractId { get; set; }
        public decimal Allowances { get; set; } = 0m;
        public decimal Deductions { get; set; } = 0m;
        public string? Notes { get; set; }
    }
}
