using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs
{
    public class CrewPayrollResponse
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int PersonId { get; set; }
        public string PersonFullName { get; set; } = null!;
        public int VesselId { get; set; }
        public string VesselName { get; set; } = null!;
        public int PayrollMonth { get; set; }
        public int WorkingDays { get; set; }
        public decimal BasicWage { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal RemainingBalance { get; set; }
        public bool IsFullyPaid { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public IReadOnlyList<DisbursementResponse> Disbursements { get; set; } = new List<DisbursementResponse>();
    }
}
