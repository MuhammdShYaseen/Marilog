using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Reports.Contract
{
    public class ContractReport
    {
        public int TotalContracts { get; init; }
        public int ActiveContracts { get; init; }
        public int DraftContracts { get; init; }
        public int ExpiredContracts { get; init; }
        public int TerminatedContracts { get; init; }
        public int SuspendedContracts { get; init; }

        public List<ContractSummary> ExpiringWithin30Days { get; init; } = [];
        public List<ContractSummary> RecentlyAmended { get; init; } = [];
    }
}
