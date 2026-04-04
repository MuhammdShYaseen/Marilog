using Marilog.Application.DTOs.Reports.Contract;
using Marilog.Domain.Entities;
using Marilog.Domain.Enumerations;


namespace Marilog.Application.Extensions
{
    internal static class ContractQueryExtensions
    {
        internal static IQueryable<AContract> ApplyFilter(
            this IQueryable<AContract> query,
            ContractFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(x =>
                    x.ContractNumber.Contains(filter.SearchTerm) ||
                    (x.Notes != null && x.Notes.Contains(filter.SearchTerm)));

            if (!string.IsNullOrWhiteSpace(filter.Type) && ContractType.TryFromName(filter.Type, out var parsed) && parsed is not null)
            {
                query = query.Where(x => x.Type.Id == parsed.Id);
            }

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

            if (filter.ExpiringWithinDays.HasValue)
            {
                // لا نستخدم IDateTimeProvider هنا — Extension لا تعرف عن Services
                // الـ threshold يُحسب في Service ويُمرَّر كـ ExpiryDateTo
            }

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
                ("status", true) => query.OrderBy(x => x.Status.Id),
                ("status", false) => query.OrderByDescending(x => x.Status.Id),
                _ => query.OrderBy(x => x.ContractNumber)
            };
    }
}
