using Marilog.Contracts.DTOs.Reports.InboxEntryReport;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    /// <summary>
    /// The one place that knows how each linkable type is labelled and searched.
    /// New type = one case in each switch + one entry in InboxLinkTargets.
    /// </summary>
    public class InboxLinkResolver
    {
        private const int MaxResults = 20;

        private readonly IRepository<Document> _documents;
        private readonly IRepository<SwiftTransfer> _swifts;
        private readonly IRepository<Payment> _payments;
        private readonly IRepository<DocumentAdjustment> _adjustments;
        private readonly IRepository<AContract> _contracts;
        private readonly IRepository<CrewContract> _crewContracts;
        private readonly IRepository<CrewPayroll> _payrolls;
        private readonly IRepository<Person> _persons;
        private readonly IRepository<Voyage> _voyages;
        private readonly IRepository<BillOfLading> _bls;
        private readonly IRepository<InboxEntry> _entries;

        public InboxLinkResolver(
            IRepository<Document> documents,
            IRepository<SwiftTransfer> swifts,
            IRepository<Payment> payments,
            IRepository<DocumentAdjustment> adjustments,
            IRepository<AContract> contracts,
            IRepository<CrewContract> crewContracts,
            IRepository<CrewPayroll> payrolls,
            IRepository<Person> persons,
            IRepository<Voyage> voyages,
            IRepository<BillOfLading> bls,
            IRepository<InboxEntry> entries)
        {
            _documents = documents;
            _swifts = swifts;
            _payments = payments;
            _adjustments = adjustments;
            _contracts = contracts;
            _crewContracts = crewContracts;
            _payrolls = payrolls;
            _persons = persons;
            _voyages = voyages;
            _bls = bls;
            _entries = entries;
        }

        //==== Public ==========================================================================================

        public async Task<bool> ExistsAsync(EntityType type, int id, CancellationToken ct = default)
            => InboxLinkTargets.All.Contains(type)
               && (await GetLabelsAsync(type, [id], ct)).ContainsKey(id);

        public async Task<List<LinkCandidateResponse>> SearchAsync(EntityType type, string? term, CancellationToken ct = default)
        {
            if (!InboxLinkTargets.All.Contains(type))
                throw new InvalidOperationException($"{type} can't be linked to an inbox entry.");

            var ids = await SearchIdsAsync(type, term?.Trim() ?? string.Empty, ct);
            var labels = await GetLabelsAsync(type, ids, ct);

            return ids.Where(labels.ContainsKey)
                      .Select(id => new LinkCandidateResponse(id, labels[id]))
                      .ToList();
        }

        /// <summary>Missing ids (deleted records) are simply absent from the result.</summary>
        public async Task<Dictionary<int, string>> GetLabelsAsync(EntityType type, List<int> ids, CancellationToken ct = default)
        {
            if (ids.Count == 0) return new Dictionary<int, string>();

            return type switch
            {
                EntityType.Document => (await _documents.Query()
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => new { x.Id, x.DocNumber, x.TotalAmount, Currency = x.Currency.CurrencyCode })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"{x.DocNumber} · {x.TotalAmount:N2} {x.Currency}"),

                EntityType.SwiftTransfer => (await _swifts.Query()
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => new { x.Id, x.SwiftReference, x.Amount, Currency = x.Currency.CurrencyCode, x.TransactionDate })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"{x.SwiftReference} · {x.Amount:N2} {x.Currency} · {x.TransactionDate:dd/MM/yyyy}"),

                EntityType.Payment => (await _payments.Query()
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => new { x.Id, x.PaidAmount, Currency = x.Document.Currency.CurrencyCode, x.Document.DocNumber, x.PaymentDate })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"{x.PaidAmount:N2} {x.Currency} · {x.DocNumber} · {x.PaymentDate:dd/MM/yyyy}"),

                EntityType.DocumentAdjustment => (await (
                        from a in _adjustments.Query()
                        join d in _documents.Query() on a.DocumentId equals d.Id
                        where ids.Contains(a.Id)
                        select new { a.Id, a.Amount, Currency = d.Currency.CurrencyCode, d.DocNumber })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"Adj. {x.Amount:N2} {x.Currency} · {x.DocNumber}"),

                EntityType.AContract => (await _contracts.Query()
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => new { x.Id, x.ContractNumber, x.EffectiveDate })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"{x.ContractNumber} · {x.EffectiveDate:dd/MM/yyyy}"),

                EntityType.CrewContract => (await (
                        from c in _crewContracts.Query()
                        join p in _persons.Query() on c.PersonID equals p.Id
                        where ids.Contains(c.Id)
                        select new { c.Id, p.FullName, c.SignOnDate })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"{x.FullName} · Sign-on {x.SignOnDate:dd/MM/yyyy}"),

                EntityType.CrewPayroll => (await (
                        from pr in _payrolls.Query()
                        join c in _crewContracts.Query() on pr.ContractId equals c.Id
                        join p in _persons.Query() on c.PersonID equals p.Id
                        where ids.Contains(pr.Id)
                        select new { pr.Id, pr.PayrollMonth, p.FullName })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"Payroll {x.PayrollMonth:MMM yyyy} · {x.FullName}"),

                EntityType.Voyage => (await _voyages.Query()
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => new { x.Id, x.VoyageNumber, x.VoyageMonth })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"{x.VoyageNumber} · {x.VoyageMonth:MMM yyyy}"),

                EntityType.BillOfLading => (await _bls.Query()
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => new { x.Id, x.BlNumber, x.IssueDate })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"{x.BlNumber} · {x.IssueDate:dd/MM/yyyy}"),

                EntityType.InboxEntry => (await _entries.Query()
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => new { x.Id, x.Number, x.Description })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => $"{x.Number} · {Short(x.Description)}"),

                _ => new Dictionary<int, string>()
            };
        }

        //==== Privates ==========================================================================================

        private async Task<List<int>> SearchIdsAsync(EntityType type, string t, CancellationToken ct)
        {
            var all = t.Length == 0; // empty search = latest records

            return type switch
            {
                EntityType.Document => await TopIds(_documents.Query()
                    .Where(x => all || x.DocNumber.Contains(t) || (x.Reference != null && x.Reference.Contains(t)))
                    .Select(x => x.Id), ct),

                EntityType.SwiftTransfer => await TopIds(_swifts.Query()
                    .Where(x => all || x.SwiftReference.Contains(t) || (x.PaymentReference != null && x.PaymentReference.Contains(t)))
                    .Select(x => x.Id), ct),

                EntityType.Payment => await TopIds(_payments.Query()
                    .Where(x => all || x.Document.DocNumber.Contains(t))
                    .Select(x => x.Id), ct),

                EntityType.DocumentAdjustment => await TopIds(
                    from a in _adjustments.Query()
                    join d in _documents.Query() on a.DocumentId equals d.Id
                    where all || d.DocNumber.Contains(t) || (a.Reason != null && a.Reason.Contains(t))
                    select a.Id, ct),

                EntityType.AContract => await TopIds(_contracts.Query()
                    .Where(x => all || x.ContractNumber.Contains(t))
                    .Select(x => x.Id), ct),

                EntityType.CrewContract => await TopIds(
                    from c in _crewContracts.Query()
                    join p in _persons.Query() on c.PersonID equals p.Id
                    where all || p.FullName.Contains(t)
                    select c.Id, ct),

                EntityType.CrewPayroll => await TopIds(
                    from pr in _payrolls.Query()
                    join c in _crewContracts.Query() on pr.ContractId equals c.Id
                    join p in _persons.Query() on c.PersonID equals p.Id
                    where all || p.FullName.Contains(t)
                    select pr.Id, ct),

                EntityType.Voyage => await TopIds(_voyages.Query()
                    .Where(x => all || x.VoyageNumber.Contains(t))
                    .Select(x => x.Id), ct),

                EntityType.BillOfLading => await TopIds(_bls.Query()
                    .Where(x => all || x.BlNumber.Contains(t))
                    .Select(x => x.Id), ct),

                EntityType.InboxEntry => await TopIds(_entries.Query()
                    .Where(x => all || x.Number.Contains(t) || x.Description.Contains(t))
                    .Select(x => x.Id), ct),

                _ => new List<int>()
            };
        }

        private static Task<List<int>> TopIds(IQueryable<int> ids, CancellationToken ct)
            => ids.OrderByDescending(id => id).Take(MaxResults).ToListAsync(ct);

        private static string Short(string text)
            => text.Length > 40 ? text[..40] + "…" : text;
    }
}