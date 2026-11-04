using Marilog.Application.DocumentExtraction;
using Marilog.Application.Interfaces.DocumentExtraction;
using Marilog.Application.Models.DocumentExtraction;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.DocumentExtraction;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Marilog.Application.Services.ApplicationServices.DocumentExtraction
{
    public class DocumentExtractionService : IDocumentExtractionService
    {
        private readonly IPdfWordReader _reader;
        private readonly IRepository<Company> _companies;
        private readonly IRepository<Vessel> _vessels;
        private readonly IRepository<Currency> _currencies;
        private readonly IRepository<Port> _ports;
        private readonly IRepository<DocumentType> _docTypes;
        private readonly IRepository<Document> _documents;

        // Document type + the phrases (its Name and Code) searched for on the page.
        private sealed record DocTypeRef(int Id, string Name, List<string> Keywords);
        

        public DocumentExtractionService(
            IPdfWordReader reader,
            IRepository<Company> companies,
            IRepository<Vessel> vessels,
            IRepository<Currency> currencies,
            IRepository<Port> ports,
            IRepository<DocumentType> docTypes,
            IRepository<Document> documents)
        {
            _reader = reader;
            _companies = companies;
            _vessels = vessels;
            _currencies = currencies;
            _ports = ports;
            _docTypes = docTypes;
            _documents = documents;
        }

        public async Task<ExtractionResponse> ExtractAsync(IReadOnlyList<ExtractionFile> files, CancellationToken ct = default)
        {
            var notes = new List<string>();
            var extracted = new List<(string FileName, ExtractedDocument Doc)>();

            foreach (var file in files)
            {
                IReadOnlyList<PdfPageWords> pdfPages;
                try
                {
                    pdfPages = _reader.Read(file.Content);
                }
                catch (Exception)
                {
                    // One broken file must not fail the whole batch.
                    notes.Add($"{file.FileName}: not a readable PDF — skipped.");
                    continue;
                }

                var pages = new List<(int, IReadOnlyList<TextLine>, ExtractedHeader)>();

                foreach (var page in pdfPages)
                {
                    if (!page.HasTextLayer)
                    {
                        notes.Add($"{file.FileName} — page {page.Number}: no selectable text (scanned) — skipped.");
                        continue;
                    }

                    var lines = LineGrouper.Group(page.Words);
                    var header = HeaderExtractor.Extract(lines);

                    if (string.Equals(header.Title, "STATEMENT OF ACCOUNT", StringComparison.OrdinalIgnoreCase))
                    {
                        notes.Add($"{file.FileName} — page {page.Number}: statement of account — skipped.");
                        continue;
                    }

                    pages.Add((page.Number, lines, header));
                }

                foreach (var doc in DocumentGrouper.Group(pages))
                    extracted.Add((file.FileName, doc));
            }

            var lookups = await LoadLookupsAsync(ct);
            if (extracted.Count == 0)
                return new ExtractionResponse([], notes, ToDto(lookups));



            var results = new List<ExtractionResult>();
            foreach (var (fileName, doc) in extracted)
                results.Add(await ResolveAsync(fileName, doc, lookups, ct));

            // The same number twice in one batch → CreateRange would reject the whole batch.
            var duplicates = results
                .Where(r => r.Payload.DocNumber.Length > 0)
                .GroupBy(r => r.Payload.DocNumber, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (duplicates.Count > 0)
                results = results
                    .Select(r => duplicates.Contains(r.Payload.DocNumber)
                        ? r with { Unresolved = [.. r.Unresolved, $"DocNumber: '{r.Payload.DocNumber}' appears more than once in this batch"] }
                        : r)
                    .ToList();

            return new ExtractionResponse(results, notes, ToDto(lookups));
        }

        // ── Resolution ───────────────────────────────────────────────────
        private async Task<ExtractionResult> ResolveAsync(string fileName, ExtractedDocument doc, Lookups lk, CancellationToken ct)
        {
            var h = doc.Header;
            var unresolved = new List<string>();
            var warnings = new List<string>();

            var raw = string.Join('\n', doc.Lines.Select(l => l.Text));
            var normalized = ExtractionParsers.NormalizeId(raw);
            var lower = raw.ToLowerInvariant();

            // Supplier: known IBAN inside the page, then exact email address, then company name.
            var suppliers = lk.Suppliers.Where(s => s.Ids.Any(normalized.Contains)).ToList();

            if (suppliers.Count == 0)
                suppliers = lk.Suppliers.Where(s => s.Emails.Any(lower.Contains)).ToList();

            if (suppliers.Count == 0)
            {
                suppliers = lk.Suppliers
                    .Where(s => s.NameKey.Length >= 8 && normalized.Contains(s.NameKey))
                    .ToList();

                if (suppliers.Count == 1)
                    warnings.Add($"Supplier: matched by name only ({suppliers[0].Name}) — verify, and add its bank account to make it exact");
            }

            var supplierId = PickOne(suppliers, s => s.Id, s => s.Name, "Supplier",
                "no known IBAN, email or company name found — create the company or add its bank account first",
                unresolved);

            // Vessel: IMO first, then the vessel name as a whole word.
            List<VesselRef> vessels = h.Imo is null ? [] : lk.Vessels.Where(v => v.Imo == h.Imo).ToList();
            if (vessels.Count == 0)
                vessels = lk.Vessels.Where(v => ContainsWord(raw, StripVesselPrefix(v.Name))).ToList();

            var vesselId = PickOne(vessels, v => v.Id, v => v.Name, "Vessel",
                "no known vessel name or IMO found", warnings);

            // Currency
            var currencyId = PickOne(
                lk.Currencies.Where(c => string.Equals(c.Name, h.Currency, StringComparison.OrdinalIgnoreCase)).ToList(),
                c => c.Id, c => c.Name, "Currency", $"'{h.Currency ?? "none"}' not found in currencies", unresolved);

            // DocType from the page title
            // DocType: the keyword appearing first on the page wins; on the same position, the longer one.
            var hits = lk.DocTypes
                .SelectMany(d => d.Keywords.Select(k => (Type: d, Index: IndexOfWord(raw, k), k.Length)))
                .Where(x => x.Index >= 0)
                .OrderBy(x => x.Index)
                .ThenByDescending(x => x.Length)
                .ToList();

            var bestTypes = hits
                .Where(x => x.Index == hits[0].Index && x.Length == hits[0].Length)
                .Select(x => x.Type)
                .DistinctBy(t => t.Id)
                .ToList();

            var docTypeId = PickOne(bestTypes, d => d.Id, d => d.Name, "DocType",
                "no document type keyword found on the page — add keywords to the document type", unresolved);

            // Port (optional): longest port name contained in the port text, compared without spaces.
            int? portId = null;
            if (h.PortText is not null)
            {
                var portKey = ExtractionParsers.NormalizeId(h.PortText);
                var port = lk.Ports
                    .Where(p => ExtractionParsers.NormalizeId(p.Name) is { Length: >= 3 } key && portKey.Contains(key))
                    .OrderByDescending(p => p.Name.Length)
                    .FirstOrDefault();

                if (port is null) warnings.Add($"Port: '{h.PortText}' not found in ports");
                else portId = port.Id;
            }

            if (h.DocNumber is null)
                unresolved.Add("DocNumber: not found in the document");
            else if (await _documents.Query().AnyAsync(d => d.DocNumber == h.DocNumber, ct))
                unresolved.Add($"DocNumber: '{h.DocNumber}' already exists in Marilog");

            if (h.DocDate is null) unresolved.Add("DocDate: not found in the document");
            if (h.TotalAmount is null) unresolved.Add("TotalAmount: not found in the document");
            if (lk.OwnCompanyId is null) warnings.Add("Buyer: no single company marked as own company");

            var payload = new CreateDocumentRequest
            {
                DocNumber = h.DocNumber ?? "",
                DocTypeId = docTypeId ?? 0,
                DocDate = h.DocDate ?? default,
                CurrencyId = currencyId ?? 0,
                TotalAmount = h.TotalAmount ?? 0,
                SupplierId = supplierId,
                BuyerId = lk.OwnCompanyId,
                VesselId = vesselId,
                PortId = portId,
                Side = FinancialSide.Expense,
                DocDue = h.DocDue
            };

            return new ExtractionResult(fileName, doc.Pages, payload, unresolved, warnings);
        }

        // Exactly one match → its id. Zero or several → null + a reason.
        private static int? PickOne<T>(List<T> matches, Func<T, int> id, Func<T, string> name,
            string field, string notFound, List<string> sink)
        {
            if (matches.Count == 1) return id(matches[0]);

            sink.Add(matches.Count == 0
                ? $"{field}: {notFound}"
                : $"{field}: several matches — {string.Join(", ", matches.Select(name))}");
            return null;
        }

        private static bool ContainsWord(string text, string? word) =>
            !string.IsNullOrWhiteSpace(word) &&
            Regex.IsMatch(text, $@"(?<![A-Z0-9]){Regex.Escape(word.Trim())}(?![A-Z0-9])", RegexOptions.IgnoreCase);

        private static string StripVesselPrefix(string? name) =>
            name is null
                ? ""
                : Regex.Replace(name.Trim(), @"^(?:M\.?\s?V\.?|M/V)\s+", "", RegexOptions.IgnoreCase);

        // ── Lookups (loaded once per file) ───────────────────────────────
        private sealed record SupplierRef(int Id, string Name, string NameKey, List<string> Ids, List<string> Emails);
        private sealed record VesselRef(int Id, string Name, string? Imo);
        private sealed record NamedRef(int Id, string Name);

        private sealed class Lookups
        {
            public int? OwnCompanyId { get; init; }
            public List<SupplierRef> Suppliers { get; init; } = [];
            public List<VesselRef> Vessels { get; init; } = [];
            public List<NamedRef> Currencies { get; init; } = [];
            public List<NamedRef> Ports { get; init; } = [];
            public List<DocTypeRef> DocTypes { get; init; } = [];
        }

        private async Task<Lookups> LoadLookupsAsync(CancellationToken ct)
        {
            var companies = await _companies.Query()
                .Select(c => new
                {
                    c.Id,
                    c.CompanyName,
                    c.IsOwnCompany,
                    Ibans = c.BankAccounts.Select(b => b.IBAN).ToList(),
                    // If BankAccount has an account-number field, add it here and concat it below.
                    Emails = c.Emails.Select(e => e.Address).ToList()
                })
                .ToListAsync(ct);

            var own = companies.Where(c => c.IsOwnCompany).Select(c => c.Id).ToList();

            return new Lookups
            {
                OwnCompanyId = own.Count == 1 ? own[0] : null,

                Suppliers = companies
                    .Where(c => !c.IsOwnCompany)
                    .Select(c => new SupplierRef(
                        c.Id,
                        c.CompanyName,
                        ExtractionParsers.NormalizeId(c.CompanyName),
                        // Short ids would match by accident inside other numbers.
                        c.Ibans
                            .Select(ExtractionParsers.NormalizeId)
                            .Where(x => x.Length >= 8)
                            .Distinct()
                            .ToList(),
                        c.Emails
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .Select(e => e.Trim().ToLowerInvariant())
                            .ToList()))
                    .ToList(),

                Vessels = await _vessels.Query().Select(v => new VesselRef(v.Id, v.VesselName, v.IMONumber)).ToListAsync(ct),
                Currencies = await _currencies.Query().Select(c => new NamedRef(c.Id, c.CurrencyCode)).ToListAsync(ct),
                Ports = await _ports.Query().Select(p => new NamedRef(p.Id, p.PortName)).ToListAsync(ct),
                DocTypes = (await _docTypes.Query()
        .Select(d => new { d.Id, d.Code, d.Name })
        .ToListAsync(ct))
    .Select(d => new DocTypeRef(
        d.Id,
        d.Name,
        new[] { d.Name, d.Code.Replace('_', ' ') }
            .Where(k => !string.IsNullOrWhiteSpace(k) && k.Trim().Length >= 3)
            .Select(k => k.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()))
    .ToList()
            };
        }

        public Task<IReadOnlyList<ExtractionDebugPage>> DebugAsync(byte[] pdf, CancellationToken ct = default)
        {
            IReadOnlyList<ExtractionDebugPage> pages = _reader.Read(pdf)
                .Select(page =>
                {
                    if (!page.HasTextLayer)
                        return new ExtractionDebugPage(page.Number, false, [], null);

                    var lines = LineGrouper.Group(page.Words);

                    return new ExtractionDebugPage(
                        page.Number,
                        true,
                        lines.Select(l => string.Join("  |  ", l.Segments())).ToList(),
                        HeaderExtractor.Extract(lines));
                })
                .ToList();

            return Task.FromResult(pages);
        }
        private static int IndexOfWord(string text, string phrase)
        {
            var pattern = Regex.Escape(phrase).Replace(@"\ ", @"\s+");
            var m = Regex.Match(text, $@"(?<![A-Z0-9]){pattern}(?![A-Z0-9])", RegexOptions.IgnoreCase);
            return m.Success ? m.Index : -1;
        }

        private static ExtractionLookups ToDto(Lookups lk)
        {
            static List<LookupItem> Sorted(IEnumerable<LookupItem> items) => items.OrderBy(i => i.Name).ToList();

            return new ExtractionLookups(
                Sorted(lk.DocTypes.Select(d => new LookupItem(d.Id, d.Name))),
                Sorted(lk.Currencies.Select(c => new LookupItem(c.Id, c.Name))),
                Sorted(lk.Suppliers.Select(s => new LookupItem(s.Id, s.Name))),
                Sorted(lk.Vessels.Select(v => new LookupItem(v.Id, v.Name))),
                Sorted(lk.Ports.Select(p => new LookupItem(p.Id, p.Name))));
        }
    }
}