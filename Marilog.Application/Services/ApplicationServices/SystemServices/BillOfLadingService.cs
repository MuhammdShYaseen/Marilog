using Marilog.Contracts.DTOs.Requests.BillOfLadingDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    // ═══════════════════════════════════════════════════════════════════════════════
    // Implementation — Application Layer
    // ═══════════════════════════════════════════════════════════════════════════════

    public class BillOfLadingService : IBillOfLadingService
    {
        private readonly IRepository<BillOfLading> _repo;

        public BillOfLadingService(IRepository<BillOfLading> repo)
        {
            _repo = repo;
        }

        // ── Mapping ──────────────────────────────────────────────────────────────
        private static readonly Expression<Func<BillOfLading, BillOfLadingResponse>> ToResponse = b =>
            new BillOfLadingResponse
            {
                Id = b.Id,
                VoyageId = b.VoyageID,
                VoyageNumber = b.Voyage.VoyageNumber,
                BlNumber = b.BlNumber,
                BlType = b.BlType.ToString(),
                IssuanceType = b.IssuanceType.ToString(),
                ShipperCompany = new CompanyResponse
                {
                   Id =  b.ShipperCompany.Id,
                   Name = b.ShipperCompany.CompanyName,
                }
                ,
                ConsigneeCompany = b.ConsigneeCompany == null ? null : new CompanyResponse
                {
                    Id = b.ConsigneeCompany.Id,
                    Name = b.ConsigneeCompany.CompanyName,
                },
                ConsigneeToOrder = b.ConsigneeToOrder,
                NotifyPartyCompany = b.NotifyPartyCompany == null ? null : new CompanyResponse
                {
                    Id = b.NotifyPartyCompany.Id,
                    Name = b.NotifyPartyCompany.CompanyName
                },
                CarrierCompany = new CompanyResponse
                {
                    Id = b.CarrierCompany.Id,
                    Name = b.CarrierCompany.CompanyName
                },
                PortOfLoading = new PortResponse
                {
                     Id  = b.PortOfLoading.Id,
                     Name = b.PortOfLoading.PortName,
                     CountryName = b.PortOfLoading.Country!.CountryName ?? ""
                },
                PortOfDischarge = new PortResponse
                {
                    Id = b.PortOfDischarge.Id,
                    Name = b.PortOfDischarge.PortName,
                    CountryName = b.PortOfDischarge.Country!.CountryName ?? ""
                },
                PlaceOfReceipt = b.PlaceOfReceipt == null ? null : new PortResponse
                {
                    Id = b.PlaceOfReceipt.Id,
                    Name = b.PlaceOfReceipt.PortName,
                    CountryName = b.PlaceOfReceipt.Country!.CountryName ?? ""
                },
                PlaceOfDelivery = b.PlaceOfDelivery == null ? null : new PortResponse
                {
                    Id = b.PlaceOfDelivery.Id,
                    Name = b.PlaceOfDelivery.PortName,
                    CountryName = b.PlaceOfDelivery.Country!.CountryName ?? ""
                },
                CargoDescription = b.CargoDescription,
                HsCode = b.HsCode,
                GrossWeightMT = b.GrossWeightMT,
                VolumeM3 = b.VolumeM3,
                PackageCount =b.PackageCount,
                PackageType = b.PackageType,
                MarksAndNumbers = b.MarksAndNumbers,
                FreightTerms = b.FreightTerms.ToString(),
                FreightAmount = b.FreightAmount,
                Incoterms = b.Incoterms,
                IssueDate = b.IssueDate,
                PlaceOfIssue = b.PlaceOfIssue,
                OnBoardDate = b.OnBoardDate,
                OriginalCopiesCount = b.OriginalCopiesCount,
                MasterBlId =  b.MasterBlID,
                MasterBlNumber = b.MasterBl == null ? null : b.MasterBl.BlNumber,
                Notes = b.Notes,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            };

        // ── Queries ──────────────────────────────────────────────────────────────
        public async Task<BillOfLadingResponse> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(b => b.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct)
                ?? throw new NullReferenceException(nameof(BillOfLading) + id.ToString());
        }

        public async Task<IReadOnlyList<BillOfLadingResponse>> GetByVoyageAsync(int voyageId, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(b => b.VoyageID == voyageId)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────
        public async Task<BillOfLadingResponse> CreateAsync(CreateBillOfLadingRequest r, CancellationToken ct = default)
        {
            var bl = BillOfLading.Create(
                r.VoyageId, r.BlNumber, r.BlType, r.IssuanceType,
                r.ShipperCompanyId, r.CarrierCompanyId,
                r.PortOfLoadingId, r.PortOfDischargeId,
                r.CargoDescription, r.GrossWeightMT, r.FreightTerms, r.IssueDate,
                r.ConsigneeCompanyId, r.ConsigneeToOrder, r.NotifyPartyCompanyId,
                r.HsCode, r.VolumeM3, r.PackageCount, r.PackageType, r.MarksAndNumbers,
                r.FreightAmount, r.Incoterms, r.PlaceOfIssue, r.OnBoardDate,
                r.OriginalCopiesCount, r.MasterBlId,
                r.PlaceOfReceiptPortId, r.PlaceOfDeliveryPortId, r.Notes);

            await _repo.AddAsync(bl, ct);
            await _repo.SaveChangesAsync(ct);

            return await GetByIdAsync(bl.Id, ct);
        }

        public async Task<BillOfLadingResponse> UpdateAsync(int id, UpdateBillOfLadingRequest r, CancellationToken ct = default)
        {
            var bl = await _repo.GetByIdAsync(id, ct)
                ?? throw new NullReferenceException(nameof(BillOfLading) + id.ToString());

            bl.Update(
                r.BlType, r.ShipperCompanyId, r.CarrierCompanyId,
                r.PortOfLoadingId, r.PortOfDischargeId,
                r.CargoDescription, r.GrossWeightMT, r.FreightTerms, r.IssueDate,
                r.ConsigneeCompanyId, r.ConsigneeToOrder, r.NotifyPartyCompanyId,
                r.HsCode, r.VolumeM3, r.PackageCount, r.PackageType, r.MarksAndNumbers,
                r.FreightAmount, r.Incoterms, r.PlaceOfIssue, r.OnBoardDate,
                r.OriginalCopiesCount, r.PlaceOfReceiptPortId, r.PlaceOfDeliveryPortId, r.Notes);

            await _repo.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var bl = await _repo.GetByIdAsync(id, ct)
                ?? throw new NullReferenceException(nameof(BillOfLading) + id.ToString());

            _repo.HardDelete(bl);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Sensitive Operations ─────────────────────────────────────────────────
        public async Task<BillOfLadingResponse> ChangeBlNumberAsync(int id, ChangeBlNumberRequest r, CancellationToken ct = default)
        {
            var bl = await _repo.GetByIdAsync(id, ct)
                ?? throw new NullReferenceException(nameof(BillOfLading) + id.ToString());

            bl.ChangeBlNumber(r.NewBlNumber);
            await _repo.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct);
        }

        public async Task<BillOfLadingResponse> ChangeIssuanceTypeAsync(int id, ChangeIssuanceTypeRequest r, CancellationToken ct = default)
        {
            var bl = await _repo.GetByIdAsync(id, ct)
                ?? throw new NullReferenceException(nameof(BillOfLading) + id.ToString());

            bl.ChangeIssuanceType(r.NewIssuanceType, r.MasterBlId);
            await _repo.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct);
        }

        public async Task<BillOfLadingResponse> LinkToMasterBlAsync(int id, LinkToMasterBlRequest r, CancellationToken ct = default)
        {
            var bl = await _repo.GetByIdAsync(id, ct)
                ?? throw new NullReferenceException(nameof(BillOfLading) + id.ToString());

            bl.LinkToMasterBl(r.MasterBlId);
            await _repo.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct);
        }
    }
}
