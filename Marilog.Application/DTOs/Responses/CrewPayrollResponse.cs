using Marilog.Kernel.Enums;


namespace Marilog.Application.DTOs.Responses
{
    public class CrewPayrollResponse
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int PersonId { get; set; }
        public string? PersonFullName { get; set; } = null!;
        public int VesselId { get; set; }
        public string? VesselName { get; set; } = null!;
        public DateOnly PayrollMonth { get; set; }
        public int WorkingDays { get; set; }
        public decimal BasicWage { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal RemainingBalance { get; set; }
        public bool IsFullyPaid { get; set; }
        public PayrollStatus Status { get; set; }
        public string? Notes { get; set; }
        public List<DisbursementResponse>? Disbursements { get; set; } = new List<DisbursementResponse>();
    }
}
