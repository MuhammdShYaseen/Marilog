using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Reports.Contract
{
    public sealed class ContractFilter
    {
        public string? SearchTerm { get; init; }
        public string? Type { get; init; }
        public string? Status { get; init; }
        public int? CompanyId { get; init; }
        public DateOnly? EffectiveDateFrom { get; init; }
        public DateOnly? EffectiveDateTo { get; init; }
        public DateOnly? ExpiryDateFrom { get; init; }
        public DateOnly? ExpiryDateTo { get; init; }
        public bool? HasAmendments { get; init; }
        public int? ExpiringWithinDays { get; init; }

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string SortBy { get; init; } = "ContractNumber";
        public bool Ascending { get; init; } = true;
    }
}
