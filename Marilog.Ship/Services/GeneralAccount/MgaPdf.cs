using System.Diagnostics;
using Marilog.Kernel.Enums;
using Marilog.Ship.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

namespace Marilog.Ship.Services.GeneralAccount
{
    /// <summary>
    /// The MGA as the office receives it: A4 landscape, English, one section per page
    /// (1 movements, 2 wages, 3 expenses, 4 received, 5 crew accounts, then the summary).
    /// Built only from MgaReport, so the PDF always shows exactly what the MGA page shows.
    /// A draft carries a DRAFT mark on every page.
    /// </summary>
    public static class MgaPdf
    {
        /// <summary>Writes the PDF to the data folder (\MGA, so it is in every backup) and opens it.</summary>
        public static string SaveAndOpen(MgaReport report)
        {
            var folder = Path.Combine(ShipDatabase.DataFolder, "MGA");
            Directory.CreateDirectory(folder);

            var name = $"MGA {report.Code.Replace('/', '-')} {report.VesselName}{(report.IsClosed ? "" : " DRAFT")}.pdf";
            var invalid = Path.GetInvalidFileNameChars();
            name = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());

            var path = Path.Combine(folder, name);
            File.WriteAllBytes(path, Generate(report));
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return path;
        }

        public static byte[] Generate(MgaReport r)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(doc =>
            {
                Page(doc, r, "1. Ship's Movements", c => Movements(c, r));
                Page(doc, r, "2. Crew Wages", c => Wages(c, r));
                Page(doc, r, "3. Master's Expenses", c => Expenses(c, r));
                Page(doc, r, "4. Received from Owner", c => Received(c, r));
                Page(doc, r, "5. Crew Accounts", c => CrewAccounts(c, r));
                Page(doc, r, "Summary of Account", c => Summary(c, r));
            }).GeneratePdf();
        }

        // ── Page frame ───────────────────────────────────────────────────────────

        private static void Page(IDocumentContainer doc, MgaReport r, string title, Action<ColumnDescriptor> content)
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily(Fonts.Arial).FontColor(P.Text));

                if (!r.IsClosed)
                    page.Background().AlignCenter().AlignMiddle().Rotate(-25)
                        .Text("DRAFT").FontSize(120).Bold().FontColor(P.Watermark);

                page.Header().Element(c => Header(c, r));
                page.Content().PaddingTop(8).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text(title.ToUpperInvariant()).FontSize(11).Bold().FontColor(P.Primary);
                    content(col);
                });
                page.Footer().Element(c => Footer(c, r));
            });
        }

        private static void Header(IContainer container, MgaReport r)
        {
            container.BorderBottom(2).BorderColor(P.Primary).PaddingBottom(6).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(r.VesselName).FontSize(15).Bold().FontColor(P.Primary);
                    col.Item().Text(t =>
                    {
                        t.Span("Master's General Account  ").FontSize(10);
                        t.Span($"MGA {r.Code}").FontSize(10).Bold();
                        if (!r.IsClosed) t.Span("   DRAFT — NOT CLOSED").FontSize(9).Bold().FontColor(P.Error);
                    });
                });

                row.ConstantItem(260).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text($"Period {r.From:dd.MM.yyyy} – {r.To:dd.MM.yyyy}").SemiBold();
                    col.Item().AlignRight().Text($"Master: {r.MasterName ?? "—"}");
                    col.Item().AlignRight().Text($"Amounts in {r.BaseCurrencyCode} unless stated").FontSize(7.5f).FontColor(P.Muted);
                });
            });
        }

        private static void Footer(IContainer container, MgaReport r)
        {
            container.BorderTop(1).BorderColor(P.Line).PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text(r.IsClosed
                        ? $"Marilog Ship · MGA {r.Code} · closed {r.ClosedAt?.ToLocalTime():dd.MM.yyyy HH:mm}"
                        : $"Marilog Ship · MGA {r.Code} · draft printed {DateTime.Now:dd.MM.yyyy HH:mm}")
                   .FontSize(7).FontColor(P.Faint);

                row.ConstantItem(90).AlignRight().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(7).FontColor(P.Faint));
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            });
        }

        // ── Page 1 ───────────────────────────────────────────────────────────────

        private static void Movements(ColumnDescriptor col, MgaReport r)
        {
            // A plain summary before the numbers.
            var s = r.Summary;
            col.Item().Row(row =>
            {
                row.Spacing(8);
                Tile(row.RelativeItem(), "Previous balance", M(s.PreviousBalance), null, P.Primary);
                Tile(row.RelativeItem(), "Closing balance", M(s.ClosingBalance), Favour(s.ClosingBalance), P.Primary);
                Tile(row.RelativeItem(), "Owed to crew", M(s.CrewBalances), "crew balances, page 5", P.Warning);
                Tile(row.RelativeItem(), "Final check", M(s.Difference),
                    s.Difference == 0 ? "the account balances" : "see the reason on the summary page",
                    s.Difference == 0 ? P.Success : P.Error);
            });

            if (r.Movements.Count == 0) { None(col, "No port calls in this period."); return; }

            var hasPurpose = r.Movements.Any(x => !string.IsNullOrWhiteSpace(x.Purpose));
            col.Item().Element(c => Grid(c, r.Movements,
            [
                new("Voyage", 2, x => x.VoyageNumber),
                new("Port", 3, x => x.PortName),
                new("Arrival", 2, x => When(x.Arrival)),
                new("Departure", 2, x => When(x.Departure)),
                .. hasPurpose ? new Col<MgaMovementLine>[] { new("Purpose", 4, x => x.Purpose ?? "") } : []
            ]));
        }

        // ── Page 2 ───────────────────────────────────────────────────────────────

        private static void Wages(ColumnDescriptor col, MgaReport r)
        {
            Note(col, "Crew whose wages are on the master's account. Wage rule (same as Marilog payroll): a full calendar " +
                      "month = the monthly wage; a part month = monthly wage × days ÷ 30. Net = wages + extras − deductions.");

            if (r.Wages.Count == 0) { None(col, "No crew paid through the master in this period."); return; }

            var hasExtras = r.Wages.Any(x => x.Extras != 0);
            var hasDeductions = r.Wages.Any(x => x.Deductions != 0);
            col.Item().Element(c => Grid(c, r.Wages,
            [
                new("Crew member", 4, x => x.CrewName, Total: "Total"),
                new("Rank", 2, x => x.RankName),
                new("Wage / month", 2, x => M(x.MonthlyWage), Right: true),
                new("Days", 1, x => x.Days.ToString(), Right: true),
                new("Wages", 2, x => M(x.Wages), Right: true, Total: M(r.Wages.Sum(x => x.Wages))),
                .. hasExtras ? new Col<MgaWageLine>[] { new("Extras", 2, x => M(x.Extras), Right: true, Total: M(r.Wages.Sum(x => x.Extras))) } : [],
                .. hasDeductions ? new Col<MgaWageLine>[] { new("Deductions", 2, x => M(x.Deductions), Right: true, Total: M(r.Wages.Sum(x => x.Deductions))) } : [],
                new("Net", 2, x => M(x.Net), Right: true, Total: M(r.Wages.Sum(x => x.Net))),
            ], totalRow: true));
        }

        // ── Page 3 ───────────────────────────────────────────────────────────────

        private static void Expenses(ColumnDescriptor col, MgaReport r)
        {
            if (r.Expenses.Count == 0) None(col, "No payments in this period.");
            else col.Item().Element(c => ExpenseGrid(c, r, r.Expenses, totalRow: true));

            if (r.CigarettePurchases.Count == 0) return;
            col.Item().PaddingTop(6).Text("CIGARETTES BOUGHT").FontSize(8).Bold().FontColor(P.Muted);
            Note(col, "Not an expense: the ship's cash became cigarettes on board (see the summary page).");
            col.Item().Element(c => ExpenseGrid(c, r, r.CigarettePurchases, totalRow: false));
        }

        private static void ExpenseGrid(IContainer container, MgaReport r, IReadOnlyList<MgaExpenseLine> lines, bool totalRow)
        {
            var hasPort = lines.Any(x => !string.IsNullOrWhiteSpace(x.PortName));
            var foreign = lines.Any(x => x.CurrencyCode != r.BaseCurrencyCode);

            Grid(container, lines,
            [
                new("Paid on", 2, x => x.Date.ToString("dd.MM.yyyy"), Total: "Total"),
                new("Document", 2, x => x.DocNumber),
                new("Type", 2, x => x.DocTypeName),
                new("Supplier", 4, x => x.SupplierName ?? ""),
                .. hasPort ? new Col<MgaExpenseLine>[] { new("Port", 2, x => x.PortName ?? "") } : [],
                new("Paid with", 2, x => MethodLabel(x.Method)),
                .. foreign ? new Col<MgaExpenseLine>[] { new("Amount", 2, x => $"{M(x.Amount)} {x.CurrencyCode}", Right: true) } : [],
                new(r.BaseCurrencyCode, 2, x => M(x.BaseAmount), Right: true, Total: M(lines.Sum(x => x.BaseAmount))),
            ], totalRow);
        }

        // ── Page 4 ───────────────────────────────────────────────────────────────

        private static void Received(ColumnDescriptor col, MgaReport r)
        {
            if (r.Received.Count == 0) { None(col, "Nothing received in this period."); return; }

            var hasPort = r.Received.Any(x => !string.IsNullOrWhiteSpace(x.PortName));
            var hasReference = r.Received.Any(x => !string.IsNullOrWhiteSpace(x.Reference));
            var foreign = r.Received.Any(x => x.CurrencyCode != r.BaseCurrencyCode);

            col.Item().Element(c => Grid(c, r.Received,
            [
                new("Date", 2, x => x.Date.ToString("dd.MM.yyyy"), Total: "Total"),
                new("Description", 7, x => x.Description),
                .. hasPort ? new Col<MgaReceivedLine>[] { new("Port", 2, x => x.PortName ?? "") } : [],
                .. hasReference ? new Col<MgaReceivedLine>[] { new("Reference", 2, x => x.Reference ?? "") } : [],
                .. foreign ? new Col<MgaReceivedLine>[] { new("Amount", 2, x => $"{M(x.Amount)} {x.CurrencyCode}", Right: true) } : [],
                new(r.BaseCurrencyCode, 2, x => M(x.BaseAmount), Right: true, Total: M(r.Received.Sum(x => x.BaseAmount))),
            ], totalRow: true));
        }

        // ── Page 5 ───────────────────────────────────────────────────────────────

        private static void CrewAccounts(ColumnDescriptor col, MgaReport r)
        {
            var master = r.CrewAccounts.Where(x => x.WagesPaidBy == WagesPaidBy.Master).ToList();
            var office = r.CrewAccounts.Where(x => x.WagesPaidBy == WagesPaidBy.Office).ToList();

            Note(col, "Balance + is owed to the crew member, − is an advance. The transactions of each crew member follow the table.");

            if (master.Count > 0)
            {
                // Columns that are 0 for everyone are left out.
                bool HasAny(Func<MgaCrewAccount, decimal> f) => master.Any(x => f(x) != 0);
                col.Item().Element(c => Grid(c, master,
                [
                    new("Crew member", 4, x => x.CrewName, Total: "Total"),
                    new("Rank", 2, x => x.RankName),
                    new("Previous", 2, x => M(x.PreviousBalance), Right: true, Total: M(master.Sum(x => x.PreviousBalance))),
                    new("Wages", 2, x => M(x.Wages), Right: true, Total: M(master.Sum(x => x.Wages))),
                    .. HasAny(x => x.Extras) ? new Col<MgaCrewAccount>[] { new("Extras", 2, x => M(x.Extras), Right: true, Total: M(master.Sum(x => x.Extras))) } : [],
                    .. HasAny(x => x.Cash) ? new Col<MgaCrewAccount>[] { new("Cash", 2, x => M(x.Cash), Right: true, Total: M(master.Sum(x => x.Cash))) } : [],
                    .. HasAny(x => x.Cigarettes) ? new Col<MgaCrewAccount>[] { new("Cigarettes", 2, x => M(x.Cigarettes), Right: true, Total: M(master.Sum(x => x.Cigarettes))) } : [],
                    .. HasAny(x => x.Other) ? new Col<MgaCrewAccount>[] { new("Other", 2, x => M(x.Other), Right: true, Total: M(master.Sum(x => x.Other))) } : [],
                    .. HasAny(x => x.PaidAshore) ? new Col<MgaCrewAccount>[] { new("Paid ashore", 2, x => M(x.PaidAshore), Right: true, Total: M(master.Sum(x => x.PaidAshore))) } : [],
                    new("Balance", 2, x => M(x.Balance), Right: true, Total: M(master.Sum(x => x.Balance))),
                ], totalRow: true));
            }

            var officeDraws = office.Where(x => x.Cash != 0 || x.Cigarettes != 0 || x.Other != 0).ToList();
            if (officeDraws.Count > 0)
            {
                col.Item().PaddingTop(6).Text("FOR OFFICE DEDUCTION (WAGES PAID BY THE OFFICE)").FontSize(8).Bold().FontColor(P.Muted);
                Note(col, "Cash and cigarettes are credited to the master; the office deducts them from the wages. Other deductions are for information.");
                col.Item().Element(c => Grid(c, officeDraws,
                [
                    new("Crew member", 4, x => x.CrewName, Total: "Total"),
                    new("Rank", 2, x => x.RankName),
                    new("Cash", 2, x => M(x.Cash), Right: true, Total: M(officeDraws.Sum(x => x.Cash))),
                    new("Cigarettes", 2, x => M(x.Cigarettes), Right: true, Total: M(officeDraws.Sum(x => x.Cigarettes))),
                    new("Credited to master", 2, x => M(x.Cash + x.Cigarettes), Right: true, Total: M(officeDraws.Sum(x => x.Cash + x.Cigarettes))),
                    new("Other (info)", 2, x => M(x.Other), Right: true, Total: M(officeDraws.Sum(x => x.Other))),
                ], totalRow: true));
            }

            // ── Transactions, one block per crew member ──────────────────────────
            foreach (var a in master.Concat(officeDraws).Where(x => x.Entries.Count > 0 || x.PreviousBalance != 0))
            {
                col.Item().PaddingTop(6).EnsureSpace(60).Column(block =>
                {
                    block.Item().Text($"{a.CrewName} — {a.RankName}{(a.WagesPaidBy == WagesPaidBy.Office ? " (wages paid by the office)" : "")}")
                         .SemiBold();

                    var rows = new List<MgaCrewEntry>
                    {
                        new(r.From.ToDateTime(TimeOnly.MinValue), false, "Previous balance", null, r.BaseCurrencyCode, a.PreviousBalance, a.PreviousBalance)
                    };
                    rows.AddRange(a.Entries);

                    block.Item().Element(c => Grid(c, rows,
                    [
                        new("Date", 2, x => x.HasTime ? x.Date.ToString("dd.MM.yyyy HH:mm") : x.Date.ToString("dd.MM.yyyy"), Total: r.To.ToString("dd.MM.yyyy")),
                        new("Entry", 2, x => x.Kind, Total: a.WagesPaidBy == WagesPaidBy.Master ? "Balance" : "Balance (unchanged)"),
                        new("Details", 6, x => x.Description ?? ""),
                        new("Amount", 2, x => x.Kind == "Previous balance" ? "" : $"{M(x.Amount)} {x.CurrencyCode}", Right: true),
                        new(r.BaseCurrencyCode, 2, x => M(x.BaseAmount), Right: true, Total: M(a.Balance)),
                    ], totalRow: true));
                });
            }
        }

        // ── Summary page ─────────────────────────────────────────────────────────

        private static void Summary(ColumnDescriptor col, MgaReport r)
        {
            var s = r.Summary;

            col.Item().Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem().Column(left =>
                {
                    left.Item().Text("ACCOUNT").FontSize(8).Bold().FontColor(P.Muted);
                    left.Item().Element(c => Lines(c,
                    [
                        ("Previous balance", s.PreviousBalance, false),
                        ("+ Crew wages (page 2)", s.TotalWages, false),
                        ("+ Master's expenses (page 3)", s.TotalExpenses, false),
                        .. s.OfficeDeductions != 0 ? new[] { ("+ Crew draws for office deduction (page 5)", s.OfficeDeductions, false) } : [],
                        ("− Received from owner (page 4)", -s.TotalReceived, false),
                        .. s.ExchangeDifference != 0 ? new[] { ("± Exchange difference on cash", s.ExchangeDifference, false) } : [],
                        .. s.CountDifference != 0 ? new[] { ("± Count difference (+ shortage, − surplus)", s.CountDifference, false) } : [],
                        ($"Closing balance — {Favour(s.ClosingBalance)}", s.ClosingBalance, true),
                    ]));
                });

                row.RelativeItem().Column(right =>
                {
                    right.Item().Text("FINAL CHECK").FontSize(8).Bold().FontColor(P.Muted);
                    right.Item().Element(c => Lines(c,
                    [
                        ("Closing balance", s.ClosingBalance, false),
                        ("− Crew balances (page 5)", -s.CrewBalances, false),
                        ("+ Cash on board", s.CashOnBoard, false),
                        ("+ Cigarettes on board", s.CigarettesOnBoard, false),
                        ("Difference (should be 0.00)", s.Difference, true),
                    ]));
                });
            });

            // ── Cash and cigarettes on board ─────────────────────────────────────
            var items = r.Cash.Select(c => new CashRow($"Cash {c.CurrencyCode}", c.Previous, c.Expected, c.Counted,
                    c.CurrencyCode == r.BaseCurrencyCode ? "1" : c.RateToBase.ToString("0.######"),
                    MgaMath.ToBase(c.Counted ?? c.Expected, c.RateToBase)))
                .Append(new CashRow($"Cigarettes (value, {r.BaseCurrencyCode})", r.PreviousCigarettes, r.ExpectedCigarettes,
                    r.CountedCigarettes, "", r.CountedCigarettes ?? r.ExpectedCigarettes))
                .ToList();

            col.Item().PaddingTop(6).Text("CASH AND CIGARETTES ON BOARD").FontSize(8).Bold().FontColor(P.Muted);
            col.Item().Element(c => Grid(c, items,
            [
                new("", 4, x => x.Label),
                new("Previous", 2, x => M(x.Previous), Right: true),
                new("Expected", 2, x => M(x.Expected), Right: true),
                new(r.IsClosed ? "Counted" : "Counted (not yet)", 2, x => x.Counted is { } n ? M(n) : "—", Right: true),
                new($"Rate to {r.BaseCurrencyCode}", 2, x => x.Rate, Right: true),
                new(r.BaseCurrencyCode, 2, x => M(x.Base), Right: true),
            ]));

            // ── Reasons and signature ────────────────────────────────────────────
            if (r.EndReason is not null)
                col.Item().Text(t => { t.Span("Period ends before the month end: ").SemiBold(); t.Span(r.EndReason); });
            if (r.Notes is not null)
                col.Item().Text(t => { t.Span("Reason for the difference: ").SemiBold(); t.Span(r.Notes); });

            col.Item().PaddingTop(24).Row(row =>
            {
                row.Spacing(40);
                Signature(row.RelativeItem(), $"Master: {r.MasterName ?? ""}");
                Signature(row.RelativeItem(), "Date");
                Signature(row.RelativeItem(), "Ship's stamp");
            });
        }

        private sealed record CashRow(string Label, decimal Previous, decimal Expected, decimal? Counted, string Rate, decimal Base);

        // ── Building blocks ──────────────────────────────────────────────────────

        private sealed record Col<T>(string Header, float Width, Func<T, string> Value, bool Right = false, string? Total = null);

        private static void Grid<T>(IContainer container, IReadOnlyList<T> items, IReadOnlyList<Col<T>> cols, bool totalRow = false)
        {
            container.Border(1).BorderColor(P.Line).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    foreach (var col in cols) c.RelativeColumn(col.Width);
                });

                table.Header(h =>
                {
                    foreach (var col in cols)
                        Cell(h.Cell().Background(P.HeaderBg).BorderBottom(1).BorderColor(P.LineStrong), col.Right)
                            .Text(col.Header.ToUpperInvariant()).FontSize(6.8f).SemiBold().FontColor(P.Muted);
                });

                for (var i = 0; i < items.Count; i++)
                {
                    var bg = i % 2 == 0 ? P.Surface : P.RowAlt;
                    foreach (var col in cols)
                        Cell(table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(P.Line), col.Right)
                            .Text(col.Value(items[i]));
                }

                if (!totalRow) return;
                foreach (var col in cols)
                    Cell(table.Cell().Background(P.HeaderBg).BorderTop(1).BorderColor(P.LineStrong), col.Right)
                        .Text(col.Total ?? "").SemiBold();
            });
        }

        private static IContainer Cell(IContainer container, bool right)
        {
            var cell = container.PaddingVertical(3).PaddingHorizontal(4);
            return right ? cell.AlignRight() : cell.AlignLeft();
        }

        private static void Lines(IContainer container, IReadOnlyList<(string Label, decimal Value, bool Bold)> lines)
        {
            container.Border(1).BorderColor(P.Line).Column(col =>
            {
                foreach (var (label, value, bold) in lines)
                {
                    col.Item().Background(bold ? P.HeaderBg : P.Surface)
                       .BorderBottom(0.5f).BorderColor(P.Line)
                       .PaddingVertical(4).PaddingHorizontal(6).Row(row =>
                       {
                           var text = row.RelativeItem().Text(label);
                           var number = row.ConstantItem(110).AlignRight().Text(M(value));
                           if (bold) { text.Bold(); number.Bold(); }
                       });
                }
            });
        }

        private static void Tile(IContainer container, string label, string value, string? caption, string accent)
        {
            container.Border(1).BorderColor(P.Line).BorderLeft(3).BorderColor(accent)
                .PaddingVertical(7).PaddingHorizontal(9).Column(col =>
                {
                    col.Item().Text(label.ToUpperInvariant()).FontSize(6.8f).SemiBold().FontColor(P.Muted);
                    col.Item().PaddingTop(2).Text(value).FontSize(13).Bold();
                    if (caption is not null)
                        col.Item().PaddingTop(1).Text(caption).FontSize(6.8f).FontColor(P.Faint);
                });
        }

        private static void Signature(IContainer container, string label)
        {
            container.Column(col =>
            {
                col.Item().Height(28).BorderBottom(1).BorderColor(P.LineStrong);
                col.Item().PaddingTop(3).Text(label).FontSize(7.5f).FontColor(P.Muted);
            });
        }

        private static void Note(ColumnDescriptor col, string text) =>
            col.Item().Text(text).FontSize(7.5f).FontColor(P.Muted);

        private static void None(ColumnDescriptor col, string text) =>
            col.Item().PaddingVertical(10).Text(text).Italic().FontColor(P.Muted);

        private static string M(decimal value) => value.ToString("N2");
        private static string When(DateTime? value) => value?.ToString("dd.MM.yyyy HH:mm") ?? "—";

        private static string Favour(decimal closing) =>
            closing > 0 ? "in the master's favour" : closing < 0 ? "in the owner's favour" : "settled";

        private static string MethodLabel(PaymentMethod method) => method switch
        {
            PaymentMethod.CashOnBoard or PaymentMethod.Cash => "Cash on board",
            PaymentMethod.CigarettesOnBoard => "Cigarettes on board",
            PaymentMethod.CashAtOffice => "Cash at office",
            PaymentMethod.BankTransfer => "Bank transfer",
            _ => method.ToString()
        };

        // Same palette as Marilog's office reports.
        private static class P
        {
            public const string Primary = "#0F6CBD";
            public const string Success = "#107C10";
            public const string Warning = "#8A6100";
            public const string Error = "#A4262C";
            public const string Text = "#242424";
            public const string Muted = "#616161";
            public const string Faint = "#8A8886";
            public const string Surface = "#FFFFFF";
            public const string RowAlt = "#FAFAFA";
            public const string HeaderBg = "#F3F2F1";
            public const string Line = "#E1DFDD";
            public const string LineStrong = "#C8C6C4";
            public const string Watermark = "#EDEBE9";
        }
    }
}