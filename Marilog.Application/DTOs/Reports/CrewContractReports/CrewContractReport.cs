using Marilog.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Reports.CrewContractReports
{
    public sealed class CrewContractReport
    {
        public IReadOnlyList<CrewContractResponse> Contracts { get; init; } = [];
        public int TotalCount { get; init; }
        public int ActiveCount { get; init; }
        public int InactiveCount { get; init; }

        // توزيع حسب السفينة
        public IReadOnlyList<VesselContractSummary> VesselSummary { get; init; } = [];

        // توزيع حسب الرتبة
        public IReadOnlyList<RankContractSummary> RankSummary { get; init; } = [];

        // توزيع حسب القسم
        public IReadOnlyList<DepartmentContractSummary> DepartmentSummary { get; init; } = [];
    }
}
