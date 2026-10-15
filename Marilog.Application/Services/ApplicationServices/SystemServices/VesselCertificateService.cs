
using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Rules;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class VesselCertificateService : IVesselCertificateService
    {
        private readonly IRepository<Vessel> _repo;

        public VesselCertificateService(IRepository<Vessel> repo) => _repo = repo;

        public async Task<CertificateSummaryResponse> GetSummaryAsync(int? vesselId = null,
            bool includeInactiveVessels = false, CancellationToken ct = default)
        {
            var today = CertificateRules.Today();
            var soon = today.AddDays(CertificateRules.ExpiringSoonDays);

            var vessels = _repo.Query();
            if (vesselId is not null)
                vessels = vessels.Where(v => v.Id == vesselId);
            else if (!includeInactiveVessels)
                vessels = vessels.Where(v => v.IsActive);

            var certificates = vessels.SelectMany(v => v.Certificates);

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
    }
}