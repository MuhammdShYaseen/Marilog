using Marilog.Kernel.Enums;


namespace Marilog.Application.DTOs.Reports.CrewContractReports
{
    public sealed class DepartmentContractSummary
    {
        public Department Department { get; init; }
        public int TotalContracts { get; init; }
        public int ActiveContracts { get; init; }
    }
}