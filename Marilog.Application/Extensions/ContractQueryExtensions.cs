
using Marilog.Contracts.DTOs.Reports.Contract;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enumerations;


namespace Marilog.Application.Extensions
{
    internal static class ContractQueryExtensions
    {
        internal static IQueryable<AContract> ApplyFilter(
            this IQueryable<AContract> query,
            ContractFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(x =>
                    x.ContractNumber.Contains(term) ||
                    x.ContractType.Name.Contains(term) ||
                    (x.Notes != null && x.Notes.Contains(term)));
            }

            if (filter.ContractTypeId.HasValue)
                query = query.Where(x => x.ContractTypeId == filter.ContractTypeId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                ContractStatus.TryFromName(filter.Status, out var status))
                query = query.Where(x => x.Status == status);

            if (filter.CompanyId.HasValue)
                query = query.Where(x => x.Parties.Any(p => p.CompanyId == filter.CompanyId));

            if (filter.EffectiveDateFrom.HasValue)
                query = query.Where(x => x.EffectiveDate >= filter.EffectiveDateFrom);

            if (filter.EffectiveDateTo.HasValue)
                query = query.Where(x => x.EffectiveDate <= filter.EffectiveDateTo);

            if (filter.ExpiryDateFrom.HasValue)
                query = query.Where(x => x.ExpiryDate >= filter.ExpiryDateFrom);

            if (filter.ExpiryDateTo.HasValue)
                query = query.Where(x => x.ExpiryDate <= filter.ExpiryDateTo);

            if (filter.HasAmendments.HasValue)
                query = filter.HasAmendments.Value
                    ? query.Where(x => x.Amendments.Any())
                    : query.Where(x => !x.Amendments.Any());

            return query;
        }

        internal static IQueryable<AContract> ApplySort(
            this IQueryable<AContract> query,
            ContractFilter filter)
            => (filter.SortBy.ToLower(), filter.Ascending) switch
            {
                ("contractnumber", true) => query.OrderBy(x => x.ContractNumber),
                ("contractnumber", false) => query.OrderByDescending(x => x.ContractNumber),
                ("effectivedate", true) => query.OrderBy(x => x.EffectiveDate),
                ("effectivedate", false) => query.OrderByDescending(x => x.EffectiveDate),
                ("expirydate", true) => query.OrderBy(x => x.ExpiryDate),
                ("expirydate", false) => query.OrderByDescending(x => x.ExpiryDate),
                ("type", true) => query.OrderBy(x => x.ContractType.Name),
                ("type", false) => query.OrderByDescending(x => x.ContractType.Name),
                ("status", true) => query.OrderBy(x => x.Status),
                ("status", false) => query.OrderByDescending(x => x.Status),
                ("amendmentscount", true) => query.OrderBy(x => x.Amendments.Count),
                ("amendmentscount", false) => query.OrderByDescending(x => x.Amendments.Count),
                _ => query.OrderByDescending(x => x.EffectiveDate)
            };
    }
}
