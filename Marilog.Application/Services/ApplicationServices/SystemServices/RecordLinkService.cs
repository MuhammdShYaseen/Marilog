using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    /// <summary>
    /// The one place that knows which page shows a record.
    /// A record with its own page → PageUrl. A child record → its parent's page.
    /// New type = one case in PageUrl, or one case in ResolveAsync.
    /// </summary>
    public class RecordLinkService : IRecordLinkService
    {
        private readonly IRepository<Vessel> _vessels;
        private readonly IRepository<Person> _persons;
        private readonly IRepository<Voyage> _voyages;
        private readonly IRepository<BillOfLading> _bls;
        private readonly IRepository<Payment> _payments;
        private readonly IRepository<DocumentAdjustment> _adjustments;
        private readonly IRepository<SwiftBankFee> _swiftFees;
        private readonly IRepository<CrewPayrollDisbursement> _disbursements;
        private readonly IRepository<CharterTerms> _charterTerms;
        private readonly IRepository<SofEvent> _sofEvents;
        private readonly IRepository<LaytimeException> _laytimeExceptions;
        private readonly IRepository<Comment> _comments;
        private readonly IRepository<StoredFile> _files;

        public RecordLinkService(
            IRepository<Vessel> vessels,
            IRepository<Person> persons,
            IRepository<Voyage> voyages,
            IRepository<BillOfLading> bls,
            IRepository<Payment> payments,
            IRepository<DocumentAdjustment> adjustments,
            IRepository<SwiftBankFee> swiftFees,
            IRepository<CrewPayrollDisbursement> disbursements,
            IRepository<CharterTerms> charterTerms,
            IRepository<SofEvent> sofEvents,
            IRepository<LaytimeException> laytimeExceptions,
            IRepository<Comment> comments,
            IRepository<StoredFile> files)
        {
            _vessels = vessels;
            _persons = persons;
            _voyages = voyages;
            _bls = bls;
            _payments = payments;
            _adjustments = adjustments;
            _swiftFees = swiftFees;
            _disbursements = disbursements;
            _charterTerms = charterTerms;
            _sofEvents = sofEvents;
            _laytimeExceptions = laytimeExceptions;
            _comments = comments;
            _files = files;
        }

        //==== Public ==========================================================================================

        public async Task<RecordLinkResponse?> ResolveAsync(EntityType type, int id, CancellationToken ct = default)
        {
            if (id <= 0) return null;

            // Records with their own page: the page itself reports a missing id.
            if (PageUrl(type, id) is { } url)
                return Link(type, id, url);

            return type switch
            {
                EntityType.VesselCertificate => await ParentAsync(EntityType.Vessel, _vessels.Query()
                    .SelectMany(v => v.Certificates, (v, c) => new { VesselId = v.Id, CertificateId = c.Id })
                    .Where(x => x.CertificateId == id)
                    .Select(x => x.VesselId), ct),

                EntityType.PersonCertificate => await ParentAsync(EntityType.Person, _persons.Query()
                    .SelectMany(p => p.Certificates, (p, c) => new { PersonId = p.Id, CertificateId = c.Id })
                    .Where(x => x.CertificateId == id)
                    .Select(x => x.PersonId), ct),

                EntityType.VoyageStop => await ParentAsync(EntityType.Voyage, _voyages.Query()
                    .SelectMany(v => v.Stops)
                    .Where(s => s.StopID == id)
                    .Select(s => s.VoyageID), ct, "stops"),

                EntityType.BillOfLading => await ParentAsync(EntityType.Voyage, _bls.Query()
                    .Where(x => x.Id == id).Select(x => x.VoyageID), ct, "bills"),

                EntityType.Payment => await ParentAsync(EntityType.Document, _payments.Query()
                    .Where(x => x.Id == id).Select(x => x.DocumentId), ct),

                EntityType.DocumentAdjustment => await ParentAsync(EntityType.Document, _adjustments.Query()
                    .Where(x => x.Id == id).Select(x => x.DocumentId), ct),

                EntityType.SwiftBankFee => await ParentAsync(EntityType.SwiftTransfer, _swiftFees.Query()
                    .Where(x => x.Id == id).Select(x => x.SwiftTransferId), ct),

                EntityType.CrewPayrollDisbursement => await ParentAsync(EntityType.CrewPayroll, _disbursements.Query()
                    .Where(x => x.Id == id).Select(x => x.PayrollId), ct),

                EntityType.CharterTerms => await ParentAsync(EntityType.AContract, _charterTerms.Query()
                    .Where(x => x.Id == id).Select(x => x.ContractId), ct),

                EntityType.SofEvent => await ParentAsync(EntityType.LaytimeCalculation, _sofEvents.Query()
                    .Where(x => x.Id == id).Select(x => x.LaytimeCalculationId), ct),

                EntityType.LaytimeException => await ParentAsync(EntityType.LaytimeCalculation, _laytimeExceptions.Query()
                    .Where(x => x.Id == id).Select(x => x.LaytimeCalculationId), ct),

                EntityType.Comment => await OwnerAsync(await _comments.Query()
                    .Where(x => x.Id == id)
                    .Select(x => new OwnerRef(x.EntityType, x.EntityId))
                    .FirstOrDefaultAsync(ct), ct),

                EntityType.StoredFile => await StoredFileAsync(id, ct),

                _ => null
            };
        }

        //==== Privates ==========================================================================================

        private static string? PageUrl(EntityType type, int id) => type switch
        {
            EntityType.Vessel => $"/vessels?id={id}",
            EntityType.Person => $"/persons?id={id}",
            EntityType.Company => $"/companies?id={id}",
            EntityType.Bank => $"/admin/banks?id={id}",
            EntityType.AContract => $"/contracts?id={id}",
            EntityType.Document => $"/documents?id={id}",
            EntityType.SwiftTransfer => $"/swift-transfers?id={id}",
            EntityType.InboxEntry => $"/inbox-entry?id={id}",
            EntityType.CrewContract => $"/crew-contracts?id={id}",
            EntityType.CrewPayroll => $"/payroll?id={id}",
            EntityType.Voyage => $"/voyages/{id}",
            EntityType.LaytimeCalculation => $"/operations/laytime/calculation/{id}",
            _ => null
        };

        // section is only for route pages (voyages): "/voyages/5" + "/stops".
        private static async Task<RecordLinkResponse?> ParentAsync(EntityType parentType, IQueryable<int> parentIds,
            CancellationToken ct, string? section = null)
        {
            var ids = await parentIds.Take(1).ToListAsync(ct);
            if (ids.Count == 0) return null;

            var url = PageUrl(parentType, ids[0])!;
            return Link(parentType, ids[0], section is null ? url : $"{url}/{section}");
        }

        // Comments and files belong to another record: open that record's page.
        private async Task<RecordLinkResponse?> OwnerAsync(OwnerRef? owner, CancellationToken ct)
        {
            if (owner?.Id is not int ownerId) return null;
            if (owner.Type is EntityType.NONE or EntityType.Comment or EntityType.StoredFile) return null;

            return await ResolveAsync(owner.Type, ownerId, ct);
        }

        // A file that belongs to nothing (archive upload) opens on the files page.
        private async Task<RecordLinkResponse?> StoredFileAsync(int id, CancellationToken ct)
        {
            var owner = await _files.Query()
                .Where(x => x.Id == id)
                .Select(x => new OwnerRef(x.EntityType, x.EntityId))
                .FirstOrDefaultAsync(ct);

            if (owner is null) return null;

            return await OwnerAsync(owner, ct)
                   ?? Link(EntityType.StoredFile, id, $"/storedfiles?id={id}");
        }

        private static RecordLinkResponse Link(EntityType type, int id, string url)
            => new() { EntityType = type, EntityId = id, Url = url };

        private sealed record OwnerRef(EntityType Type, int? Id);
    }
}