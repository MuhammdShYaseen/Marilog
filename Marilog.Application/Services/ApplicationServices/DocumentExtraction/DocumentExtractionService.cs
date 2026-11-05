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

        // ── Side detection labels ────────────────────────────────────────
        private static Regex[] SideLabels(params string[] patterns) =>
            patterns.Select(p => new Regex($@"^(?:{p})(?![A-Z0-9])", RegexOptions.Compiled | RegexOptions.IgnoreCase)).ToArray();

        // Who receives the money.
        private static readonly Regex[] PayeeLabels = SideLabels(
            @"ACCOUNT\s*(?:NAME|TITLE)", @"A/C\s*NAME", @"BENEFICIARY(?:\s*NAME)?", @"IN\s*FAVOU?R\s*OF", @"PAYABLE\s*TO");

        // Who is being billed.
        private static readonly Regex[] AddresseeLabels = SideLabels(
     @"INVOICE\s*TO", @"BILL(?:ED)?\s*TO", @"BUYER", @"CUSTOMER(?:\s*NAME)?", @"CHARTERERS?",
     @"COMPANY\s*NAME", @"NAME\s*OF\s*THE\s*COMPANY", @"MESS(?:RS|ES)", @"M/S", @"TO");

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

        // ── Extraction ───────────────────────────────────────────────────
        public async Task<ExtractionResponse> ExtractAsync(IReadOnlyList<ExtractionFile> files, CancellationToken ct = default)
        {
            var notes = new List<string>();
            var extracted = new List<(string FileName, ExtractedDocument Doc)>();

            // Loaded first: the header extractor needs the currency codes and symbols.
            var lookups = await LoadLookupsAsync(ct);

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

                // A payment advice, and the SWIFT message pages printed after it, is not a document to post;
                // its debtor account (ours) would also flip the side detection of the invoice it is attached to.
                var inAdvice = false;

                foreach (var page in pdfPages)
                {
                    if (!page.HasTextLayer)
                    {
                        notes.Add($"{file.FileName} — page {page.Number}: no selectable text (scanned) — skipped.");
                        continue;
                    }

                    var lines = LineGrouper.Group(page.Words);
                    var header = HeaderExtractor.Extract(lines, lookups.CurrencyCatalog);

                    if (string.Equals(header.Title, "PAYMENT ADVICE", StringComparison.OrdinalIgnoreCase))
                    {
                        inAdvice = true;
                        notes.Add($"{file.FileName} — page {page.Number}: payment advice — skipped.");
                        continue;
                    }

                    // Pages after the advice belong to it until a page starts a new document.
                    if (inAdvice && header.DocNumber is null && header.TotalAmount is null)
                    {
                        notes.Add($"{file.FileName} — page {page.Number}: part of the payment advice — skipped.");
                        continue;
                    }

                    inAdvice = false;
                    pages.Add((page.Number, lines, header));
                }

                foreach (var doc in DocumentGrouper.Group(pages))
                    extracted.Add((file.FileName, doc));
            }

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

            // Side: who is asking whom for money (bank account, payee, addressee, issuer).
            var (isRevenue, ownId, sideReasons) = DetectSide(doc.Lines, normalized, lk);
            var role = isRevenue ? "Buyer" : "Supplier";

            warnings.Add(sideReasons.Count > 0
                ? $"Side: {(isRevenue ? "Revenue" : "Expense")} — {string.Join("; ", sideReasons)}"
                : "Side: Expense (default — no side signal found on the document)");

            if (string.Equals(h.Title, "STATEMENT OF ACCOUNT", StringComparison.OrdinalIgnoreCase))
                warnings.Add("Statement of account — post it only when its detailed invoices are not recorded separately; otherwise remove this row");

            // Counterparty: known IBAN, then exact email, then company name.
            var counterparties = lk.Suppliers.Where(s => s.Ids.Any(normalized.Contains)).ToList();

            if (counterparties.Count == 0)
                counterparties = lk.Suppliers.Where(s => s.Emails.Any(lower.Contains)).ToList();

            if (counterparties.Count == 0)
            {
                counterparties = lk.Suppliers
                    .Where(s => s.NameKey.Length >= 8 && normalized.Contains(s.NameKey))
                    .ToList();

                if (counterparties.Count == 1)
                    warnings.Add($"{role}: matched by name only ({counterparties[0].Name}) — verify, and add its bank account to make it exact");
            }

            var counterpartyId = PickOne(counterparties, s => s.Id, s => s.Name, role,
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

            // DocType: the earliest Name/Code on the page wins,
            // but a more specific phrase anywhere beats a generic one it contains ("Hire Invoice" over "Invoice").
            var hits = lk.DocTypes
                .SelectMany(d => d.Keywords.Select(k => (Type: d, Keyword: k, Index: IndexOfWord(raw, k))))
                .Where(x => x.Index >= 0)
                .OrderBy(x => x.Index)
                .ThenByDescending(x => x.Keyword.Length)
                .ToList();

            var winner = hits.FirstOrDefault();
            if (hits.Count > 0)
            {
                var specific = hits.FirstOrDefault(x =>
                    x.Keyword.Length > winner.Keyword.Length &&
                    x.Keyword.Contains(winner.Keyword, StringComparison.OrdinalIgnoreCase));

                if (specific.Type is not null) winner = specific;
            }

            var bestTypes = hits
                .Where(x => x.Index == winner.Index && x.Keyword.Length == winner.Keyword.Length)
                .Select(x => x.Type)
                .DistinctBy(t => t.Id)
                .ToList();

            var docTypeId = PickOne(bestTypes, d => d.Id, d => d.Name, "DocType",
                "no document type Name or Code found on the page", unresolved);

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
            if (ownId is null) warnings.Add("Own company: none matched on the document and more than one is marked as own");

            var payload = new CreateDocumentRequest
            {
                DocNumber = h.DocNumber ?? "",
                DocTypeId = docTypeId ?? 0,
                DocDate = h.DocDate ?? default,
                CurrencyId = currencyId ?? 0,
                TotalAmount = h.TotalAmount ?? 0,
                SupplierId = isRevenue ? ownId : counterpartyId,
                BuyerId = isRevenue ? counterpartyId : ownId,
                Side = isRevenue ? FinancialSide.Revenue : FinancialSide.Expense,
                VesselId = vesselId,
                PortId = portId,
                DocDue = h.DocDue
            };

            return new ExtractionResult(fileName, doc.Pages, payload, unresolved, warnings);
        }

        // ── Side detection ───────────────────────────────────────────────
        // Score > 0 → revenue (we ask for the money), otherwise expense.
        private static (bool IsRevenue, int? OwnId, List<string> Reasons) DetectSide(
            IReadOnlyList<TextLine> lines, string normalized, Lookups lk)
        {
            var score = 0;
            int? ownId = null;
            var reasons = new List<string>();

            // A. Bank account on the document = where the money goes = the issuer.
            var ownBank = lk.Owns.FirstOrDefault(o => o.Ids.Any(normalized.Contains));
            if (ownBank is not null)
            {
                score += 3;
                ownId ??= ownBank.Id;
                reasons.Add($"own bank account ({ownBank.Name})");
            }

            var otherBank = lk.Suppliers.FirstOrDefault(s => s.Ids.Any(normalized.Contains));
            if (otherBank is not null)
            {
                score -= 3;
                reasons.Add($"bank account of {otherBank.Name}");
            }

            // B. Payee name.
            foreach (var text in LabeledTexts(lines, PayeeLabels, linesBelow: 0))
            {
                if (NameIn(lk.Owns, text) is { } own)
                {
                    score += 2;
                    ownId ??= own.Id;
                    reasons.Add($"payee is {own.Name}");
                }
                else if (NameIn(lk.Suppliers, text) is { } other)
                {
                    score -= 2;
                    reasons.Add($"payee is {other.Name}");
                }
            }

            // C. Addressee: billing someone else = revenue, being billed = expense.
            foreach (var text in LabeledTexts(lines, AddresseeLabels, linesBelow: 2))
            {
                if (NameIn(lk.Owns, text) is { } own)
                {
                    score -= 2;
                    ownId ??= own.Id;
                    reasons.Add($"addressed to {own.Name}");
                }
                else if (NameIn(lk.Suppliers, text) is { } other)
                {
                    score += 2;
                    reasons.Add($"addressed to {other.Name}");
                }
            }

            // D. Issuer: the letterhead, i.e. the first line only.
            // Lines 2–3 often hold the addressee ("WKA OCEAN SHIPPING - SOA"), so they say nothing about who issued it.
            var top = lines.Count > 0 ? ExtractionParsers.NormalizeId(lines[0].Text) : "";
            if (NameIn(lk.Owns, top) is { } ownTop)
            {
                score += 1;
                ownId ??= ownTop.Id;
                reasons.Add($"issued by {ownTop.Name}");
            }
            else if (NameIn(lk.Suppliers, top) is { } otherTop)
            {
                score -= 1;
                reasons.Add($"issued by {otherTop.Name}");
            }

            return (score > 0, ownId ?? lk.OwnCompanyId, reasons);
        }

        // Text after a label: rest of its segment, the next segment, and up to N lines below in the same column.
        private static IEnumerable<string> LabeledTexts(IReadOnlyList<TextLine> lines, Regex[] labels, int linesBelow)
        {
            for (var li = 0; li < lines.Count; li++)
            {
                var segments = lines[li].SegmentSpans();

                for (var i = 0; i < segments.Count; i++)
                {
                    var m = labels.Select(l => l.Match(segments[i].Text)).FirstOrDefault(x => x.Success);
                    if (m is null) continue;

                    var parts = new List<string> { segments[i].Text[m.Length..] };
                    if (i + 1 < segments.Count) parts.Add(segments[i + 1].Text);

                    for (var n = li + 1; n <= li + linesBelow && n < lines.Count; n++)
                    {
                        var aligned = lines[n].SegmentSpans().FirstOrDefault(s => Math.Abs(s.Left - segments[i].Left) < 40);
                        if (aligned is not null) parts.Add(aligned.Text);
                    }

                    yield return ExtractionParsers.NormalizeId(string.Join(' ', parts));
                }
            }
        }

        // Longest company name contained in an already-normalized text.
        private static SupplierRef? NameIn(List<SupplierRef> refs, string normalizedText) =>
            refs.Where(r => r.NameKey.Length >= 6 && normalizedText.Contains(r.NameKey))
                .OrderByDescending(r => r.NameKey.Length)
                .FirstOrDefault();

        // ── Helpers ──────────────────────────────────────────────────────
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

        private static int IndexOfWord(string text, string phrase)
        {
            var pattern = Regex.Escape(phrase).Replace(@"\ ", @"\s+");
            var m = Regex.Match(text, $@"(?<![A-Z0-9]){pattern}(?![A-Z0-9])", RegexOptions.IgnoreCase);
            return m.Success ? m.Index : -1;
        }

        private static string StripVesselPrefix(string? name) =>
            name is null
                ? ""
                : Regex.Replace(name.Trim(), @"^(?:M\.?\s?V\.?|M/V)\s+", "", RegexOptions.IgnoreCase);

        // ── Lookups (loaded once per request) ────────────────────────────
        private sealed record SupplierRef(int Id, string Name, string NameKey, List<string> Ids, List<string> Emails);
        private sealed record VesselRef(int Id, string Name, string? Imo);
        private sealed record NamedRef(int Id, string Name);

        // Document type + the phrases (its Name and Code) searched for on the page.
        private sealed record DocTypeRef(int Id, string Name, List<string> Keywords);

        private sealed class Lookups
        {
            public int? OwnCompanyId { get; init; }
            public List<SupplierRef> Owns { get; init; } = [];
            public List<SupplierRef> Suppliers { get; init; } = [];
            public List<NamedRef> Companies { get; init; } = [];
            public List<VesselRef> Vessels { get; init; } = [];
            public List<NamedRef> Currencies { get; init; } = [];
            public List<NamedRef> Ports { get; init; } = [];
            public List<DocTypeRef> DocTypes { get; init; } = [];
            public CurrencyCatalog CurrencyCatalog { get; init; } = default!;
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

            // Short ids would match by accident inside other numbers, so ids under 8 characters are dropped.
            static SupplierRef ToRef(int id, string name, IEnumerable<string?> ibans, IEnumerable<string?> emails) => new(
                id,
                name,
                ExtractionParsers.NormalizeId(ExtractionParsers.StripLegalSuffix(name)),
                ibans.Select(ExtractionParsers.NormalizeId).Where(x => x.Length >= 8).Distinct().ToList(),
                emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e!.Trim().ToLowerInvariant()).ToList());

            var own = companies.Where(c => c.IsOwnCompany).Select(c => c.Id).ToList();

            var docTypes = await _docTypes.Query()
                .Select(d => new { d.Id, d.Code, d.Name })
                .ToListAsync(ct);

            var (currencyRefs, currencyCatalog) = await LoadCurrenciesAsync(ct);

            return new Lookups
            {
                OwnCompanyId = own.Count == 1 ? own[0] : null,

                Owns = companies
                    .Where(c => c.IsOwnCompany)
                    .Select(c => ToRef(c.Id, c.CompanyName, c.Ibans, c.Emails))
                    .ToList(),

                Suppliers = companies
                    .Where(c => !c.IsOwnCompany)
                    .Select(c => ToRef(c.Id, c.CompanyName, c.Ibans, c.Emails))
                    .ToList(),

                Companies = companies.Select(c => new NamedRef(c.Id, c.CompanyName)).ToList(),

                Vessels = await _vessels.Query().Select(v => new VesselRef(v.Id, v.VesselName, v.IMONumber)).ToListAsync(ct),
                Ports = await _ports.Query().Select(p => new NamedRef(p.Id, p.PortName)).ToListAsync(ct),

                Currencies = currencyRefs,
                CurrencyCatalog = currencyCatalog,

                DocTypes = docTypes
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

        // Currency codes for resolution, plus codes and symbols for reading the page.
        private async Task<(List<NamedRef> Refs, CurrencyCatalog Catalog)> LoadCurrenciesAsync(CancellationToken ct)
        {
            var rows = await _currencies.Query()
                .Select(c => new { c.Id, c.CurrencyCode, c.Symbol })
                .ToListAsync(ct);

            return (rows.Select(c => new NamedRef(c.Id, c.CurrencyCode)).ToList(),
                    new CurrencyCatalog(rows.Select(c => (c.CurrencyCode, c.Symbol))));
        }

        private static ExtractionLookups ToDto(Lookups lk)
        {
            static List<LookupItem> Sorted(IEnumerable<LookupItem> items) => items.OrderBy(i => i.Name).ToList();

            return new ExtractionLookups(
                Sorted(lk.DocTypes.Select(d => new LookupItem(d.Id, d.Name))),
                Sorted(lk.Currencies.Select(c => new LookupItem(c.Id, c.Name))),
                Sorted(lk.Companies.Select(c => new LookupItem(c.Id, c.Name))),
                Sorted(lk.Vessels.Select(v => new LookupItem(v.Id, v.Name))),
                Sorted(lk.Ports.Select(p => new LookupItem(p.Id, p.Name))));
        }

        // ── Debug ────────────────────────────────────────────────────────
        public async Task<IReadOnlyList<ExtractionDebugPage>> DebugAsync(byte[] pdf, CancellationToken ct = default)
        {
            var (_, currencies) = await LoadCurrenciesAsync(ct);

            return _reader.Read(pdf)
                .Select(page =>
                {
                    if (!page.HasTextLayer)
                        return new ExtractionDebugPage(page.Number, false, [], null);

                    var lines = LineGrouper.Group(page.Words);

                    return new ExtractionDebugPage(
                        page.Number,
                        true,
                        lines.Select(l => string.Join("  |  ", l.Segments())).ToList(),
                        HeaderExtractor.Extract(lines, currencies));
                })
                .ToList();
        }
    }
}