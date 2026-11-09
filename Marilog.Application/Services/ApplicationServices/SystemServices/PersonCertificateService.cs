using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.PersonDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Rules;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class PersonCertificateService : IPersonCertificateService
    {
        private readonly IRepository<Person> _repo;

        public PersonCertificateService(IRepository<Person> repo) => _repo = repo;

        public async Task<CertificateSummaryResponse> GetSummaryAsync(int? personId = null,
            bool includeInactivePersons = false, CancellationToken ct = default)
        {
            var today = CertificateRules.Today();
            var soon = today.AddDays(CertificateRules.ExpiringSoonDays);

            var persons = FilterPersons(personId, includeInactivePersons);

            var certificates = persons.SelectMany(p => p.Certificates);

            var total = await certificates.CountAsync(ct);
            var expired = await certificates.CountAsync(c => c.Certificate.ExpiryDate < today, ct);
            var expiringSoon = await certificates.CountAsync(c => c.Certificate.ExpiryDate >= today
                                                               && c.Certificate.ExpiryDate <= soon, ct);
            var noExpiry = await certificates.CountAsync(c => c.Certificate.ExpiryDate == null, ct);

            return new CertificateSummaryResponse
            {
                Total = total,
                Expired = expired,
                ExpiringSoon = expiringSoon,
                NoExpiry = noExpiry,
                Valid = total - expired - expiringSoon - noExpiry
            };
        }

        public async Task<PagedResponse<CertificateResponse>> GetPagedAsync(PagedRequest paging,
            PersonCertificateFilter filter, CancellationToken ct = default)
        {
            var today = CertificateRules.Today();
            var soon = today.AddDays(CertificateRules.ExpiringSoonDays);

            var query = FilterPersons(filter.PersonId, filter.IncludeInactivePersons)
                .AsNoTracking()
                .SelectMany(p => p.Certificates, (p, c) => new { Person = p, Cert = c });

            if (filter.Type is not null)
                query = query.Where(x => x.Cert.Type == filter.Type);

            query = filter.Status switch
            {
                CertificateExpiryStatus.Expired => query.Where(x => x.Cert.Certificate.ExpiryDate < today),
                CertificateExpiryStatus.ExpiringSoon => query.Where(x => x.Cert.Certificate.ExpiryDate >= today
                                                                      && x.Cert.Certificate.ExpiryDate <= soon),
                CertificateExpiryStatus.Valid => query.Where(x => x.Cert.Certificate.ExpiryDate > soon),
                CertificateExpiryStatus.NoExpiry => query.Where(x => x.Cert.Certificate.ExpiryDate == null),
                _ => query
            };

            if (!string.IsNullOrWhiteSpace(paging.SearchTerm))
            {
                var term = paging.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Cert.Certificate.CertificateName.Contains(term) ||
                    (x.Cert.Certificate.CertificateNumber != null && x.Cert.Certificate.CertificateNumber.Contains(term)) ||
                    (x.Cert.Certificate.IssuingAuthority != null && x.Cert.Certificate.IssuingAuthority.Contains(term)) ||
                    x.Person.FullName.Contains(term));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Cert.Certificate.ExpiryDate == null)
                .ThenBy(x => x.Cert.Certificate.ExpiryDate)
                .ThenBy(x => x.Cert.Certificate.CertificateName)
                .Skip((paging.Page - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .Select(x => new CertificateResponse
                {
                    Index = x.Cert.Id,
                    CertificateName = x.Cert.Certificate.CertificateName,
                    CertificateNumber = x.Cert.Certificate.CertificateNumber,
                    IssuingAuthority = x.Cert.Certificate.IssuingAuthority,
                    IssueDate = x.Cert.Certificate.IssueDate,
                    ExpiryDate = x.Cert.Certificate.ExpiryDate,
                    Description = x.Cert.Certificate.Description,
                    PType = x.Cert.Type,
                    PId = x.Person.Id,
                    PName = x.Person.FullName,
                    VType = null,
                    VId = null,
                    VName = null
                })
                .ToListAsync(ct);

            return new PagedResponse<CertificateResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = paging.Page,
                PageSize = paging.PageSize
            };
        }

        private IQueryable<Person> FilterPersons(int? personId, bool includeInactivePersons)
        {
            var persons = _repo.Query();
            if (personId is not null)
                return persons.Where(p => p.Id == personId);
            return includeInactivePersons ? persons : persons.Where(p => p.IsActive);
        }
    }
}