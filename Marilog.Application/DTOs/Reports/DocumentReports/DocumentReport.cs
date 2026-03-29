using Marilog.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Reports.DocumentReports
{
    public class DocumentReport
    {
        public IReadOnlyList<DocumentResponse> Documents { get; set; } = Array.Empty<DocumentResponse>();
        public decimal TotalValue { get; set; }
        public int Count { get; set; }
        public IReadOnlyList<MonthlyTotal> MonthlyTotals { get; set; } = Array.Empty<MonthlyTotal>();
        public IReadOnlyList<YearlyTotal> YearlyTotals { get; set; } = Array.Empty<YearlyTotal>();
        public decimal TotalPaid { get; internal set; }
        public decimal TotalRemaining { get; internal set; }
        public object MonthlySummary { get; internal set; }
        public object SupplierSummary { get; internal set; }
        public object VesselSummary { get; internal set; }
    }
}
