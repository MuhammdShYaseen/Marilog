using Marilog.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Reports.CrewPayrollReports
{
    public class CrewPayrollReport
    {
        public IReadOnlyList<CrewPayrollResponse> Payrolls { get; set; } = Array.Empty<CrewPayrollResponse>();

        public decimal TotalGross { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal TotalRemaining { get; set; }

        public decimal AverageGross { get; set; }
        public decimal MaxGross { get; set; }
        public decimal MinGross { get; set; }

        public IReadOnlyList<MonthlyPayrollSummary> MonthlySummary { get; set; } = Array.Empty<MonthlyPayrollSummary>();
        public IReadOnlyList<VesselPayrollSummary> VesselSummary { get; set; } = Array.Empty<VesselPayrollSummary>();
        public IReadOnlyList<PersonPayrollSummary> PersonSummary { get; set; } = Array.Empty<PersonPayrollSummary>();
    }
}
