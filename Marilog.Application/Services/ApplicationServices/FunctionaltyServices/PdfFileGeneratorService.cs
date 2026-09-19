using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Reports.PaymentReports;

using Marilog.Contracts.Interfaces.Services.FunctionaltyServices;
using Marilog.Contracts.Interfaces.Services.SystemServices;

using Marilog.Kernel.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

namespace Marilog.Application.Services.ApplicationServices.FunctionaltyServices
{
    // ═══════════════════════════════════════════════════════════════════════
    //  PDF generation — Azure-portal inspired visual language.
    //
    //  Shared building blocks (Az palette, ComposeHeader, ComposeFooter,
    //  KpiTile, SectionTitle, BuildTable) are used by every report so the
    //  whole output set looks like one product.
    //
    //  Money rule for the Document Report: summary figures are in the BASE
    //  currency and are stated NET of price adjustments. The two detail
    //  tables are in each document's OWN currency — for line-by-line
    //  verification, never for cross-currency arithmetic.
    // ═══════════════════════════════════════════════════════════════════════
    public class PdfFileGeneratorService : IPdfFileGeneratorService
    {
        private readonly IBillOfLadingService _billService;

        public PdfFileGeneratorService(IBillOfLadingService billService)
        {
            _billService = billService;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  DOCUMENT REPORT
        // ═══════════════════════════════════════════════════════════════════
        public Task<byte[]> GenerateDocumentReportPdf(DocumentReport report, string title, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            QuestPDF.Settings.License = LicenseType.Community;

            var hasAdjustments = report.TotalAdjustments != 0 || report.Adjustments.Any();

            var bytes = Document.Create(container =>
            {
                // ── Page 1 — Portrait: summaries ──────────────────────────
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.4f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Az.Font).FontColor(Az.Text));

                    page.Header().Element(ComposeHeader(title, report.BaseCurrencyCode));
                    page.Content().PaddingTop(10).Element(ComposeSummaryContent(report));
                    page.Footer().Element(ComposeFooter("Document Report", report.BaseCurrencyCode));
                });

                // ── Page 2 — Landscape: line-level detail ─────────────────
                if (report.Documents.Any())
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.4f, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Az.Font).FontColor(Az.Text));

                        page.Header().Element(ComposeHeader($"{title} — Detail", report.BaseCurrencyCode));
                        page.Content().PaddingTop(10).Element(ComposeDetailContent(report, hasAdjustments));
                        page.Footer().Element(ComposeFooter("Document Report", report.BaseCurrencyCode));
                    });
                }
            }).GeneratePdf();

            return Task.FromResult(bytes);
        }

        // ── Summary content (Portrait) ─────────────────────────────────────
        private static Action<IContainer> ComposeSummaryContent(DocumentReport report) => container =>
        {
            container.Column(col =>
            {
                col.Spacing(14);

                // ── KPI tiles ─────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.Spacing(8);
                    KpiTile(row.RelativeItem(), "Total Docs", report.Count.ToString(), null, Az.Primary);
                    KpiTile(row.RelativeItem(), "Total Value", $"{report.TotalValue:N2}",
                            report.TotalAdjustments != 0 ? "net of adjustments" : null, Az.Primary);
                    KpiTile(row.RelativeItem(), "Total Paid", $"{report.TotalPaid:N2}", null, Az.Success);
                    KpiTile(row.RelativeItem(), "Remaining", $"{report.TotalRemaining:N2}", null, Az.Warning);
                });

                // ── Adjustments reconciliation ────────────────────────────
                if (report.TotalAdjustments != 0)
                {
                    col.Item().Element(SectionTitle("Price Adjustments Reconciliation"));
                    col.Item().Element(c => ReconciliationBlock(c, report));
                }

                // ── Financial overview ────────────────────────────────────
                if (report.RevenueSideSummary?.Any() == true || report.ExpenseSideSummary?.Any() == true)
                {
                    var revenue = report.RevenueSideSummary?.Sum(x => x.TotalValue) ?? 0m;
                    var expense = report.ExpenseSideSummary?.Sum(x => x.TotalValue) ?? 0m;
                    var net = report.NetPosition;

                    col.Item().Element(SectionTitle("Financial Overview"));
                    col.Item().Row(row =>
                    {
                        row.Spacing(8);
                        KpiTile(row.RelativeItem(), "Revenue", $"{revenue:N2}", null, Az.Success);
                        KpiTile(row.RelativeItem(), "Expense", $"{expense:N2}", null, Az.Error);
                        KpiTile(row.RelativeItem(), "Net Position", $"{net:N2}", "revenue minus expense",
                                net >= 0 ? Az.Success : Az.Error);
                    });
                }

                // ── Monthly breakdown ─────────────────────────────────────
                if (report.MonthlySummary.Any())
                {
                    col.Item().Element(SectionTitle("Monthly Breakdown"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Year", "Month", "Count", "Value", "Paid", "Remaining", "Revenue", "Expense", "Net"],
                        rows: report.MonthlySummary.Select(m => new string?[]
                        {
                            m.Year.ToString(),
                            System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m.Month),
                            m.Count.ToString(),
                            $"{m.TotalValue:N2}",
                            $"{m.TotalPaid:N2}",
                            $"{m.TotalRemain:N2}",
                            $"{m.Revenue:N2}",
                            $"{m.Expense:N2}",
                            $"{m.NetPosition:N2}",
                        }).ToList(),
                        rightAlignedColumns: [2, 3, 4, 5, 6, 7, 8],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [4] = (Az.Success, Az.Success),
                            [5] = (Az.Warning, Az.Warning),
                            [6] = (Az.Success, Az.Success),
                            [7] = (Az.Error, Az.Error),
                            [8] = (Az.Success, Az.Error),
                        },
                        netColumnIndex: 8,
                        netValues: report.MonthlySummary.Select(m => m.NetPosition).ToList(),
                        columnWidths: [0.9f, 1.7f, 0.8f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f]
                    ));
                }

                // ── By supplier ───────────────────────────────────────────
                if (report.SupplierSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Supplier"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Supplier", "Count", "Value", "Paid", "Remaining"],
                        rows: report.SupplierSummary.Select(s => new string?[]
                        {
                            s.SupplierName,
                            s.Count.ToString(),
                            $"{s.TotalValue:N2}",
                            $"{s.TotalPaid:N2}",
                            $"{s.TotalRemain:N2}",
                        }).ToList(),
                        rightAlignedColumns: [1, 2, 3, 4],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Az.Success, Az.Success),
                            [4] = (Az.Warning, Az.Warning),
                        },
                        columnWidths: [3.8f, 0.9f, 1.5f, 1.5f, 1.5f]
                    ));
                }

                // ── By vessel ─────────────────────────────────────────────
                if (report.VesselSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Vessel"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Vessel", "Count", "Value", "Paid", "Remaining", "Revenue", "Expense", "Net"],
                        rows: report.VesselSummary.Select(v => new string?[]
                        {
                            v.VesselName,
                            v.Count.ToString(),
                            $"{v.TotalValue:N2}",
                            $"{v.TotalPaid:N2}",
                            $"{v.TotalRemain:N2}",
                            $"{v.Revenue:N2}",
                            $"{v.Expense:N2}",
                            $"{v.NetPosition:N2}",
                        }).ToList(),
                        rightAlignedColumns: [1, 2, 3, 4, 5, 6, 7],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Az.Success, Az.Success),
                            [4] = (Az.Warning, Az.Warning),
                            [5] = (Az.Success, Az.Success),
                            [6] = (Az.Error, Az.Error),
                            [7] = (Az.Success, Az.Error),
                        },
                        netColumnIndex: 7,
                        netValues: report.VesselSummary.Select(v => v.NetPosition).ToList(),
                        columnWidths: [2.2f, 0.9f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f]
                    ));
                }

                // ── By voyage ─────────────────────────────────────────────
                if (report.VoyageSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Voyage"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Voyage", "Count", "Value", "Paid", "Remaining", "Revenue", "Expense", "Net"],
                        rows: report.VoyageSummary.Select(v => new string?[]
                        {
                            v.VoyageNumber + " " + v.VoyageSummary,
                            v.Count.ToString(),
                            $"{v.TotalValue:N2}",
                            $"{v.TotalPaid:N2}",
                            $"{v.TotalRemain:N2}",
                            $"{v.Revenue:N2}",
                            $"{v.Expense:N2}",
                            $"{v.NetPosition:N2}",
                        }).ToList(),
                        rightAlignedColumns: [1, 2, 3, 4, 5, 6, 7],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Az.Success, Az.Success),
                            [4] = (Az.Warning, Az.Warning),
                            [5] = (Az.Success, Az.Success),
                            [6] = (Az.Error, Az.Error),
                            [7] = (Az.Success, Az.Error),
                        },
                        netColumnIndex: 7,
                        netValues: report.VoyageSummary.Select(v => v.NetPosition).ToList(),
                        columnWidths: [2.2f, 0.9f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f]
                    ));
                }
            });
        };

        // ── Detail content (Landscape) ─────────────────────────────────────
        private static Action<IContainer> ComposeDetailContent(DocumentReport report, bool hasAdjustments) => container =>
        {
            container.Column(col =>
            {
                col.Spacing(14);

                // ── Price adjustments — document currency ─────────────────
                if (report.Adjustments.Any())
                {
                    col.Item().Element(SectionTitle($"Price Adjustments ({report.Adjustments.Count})"));
                    col.Item().Element(c => NoteLine(c,
                        "Settlements agreed after issue. Amounts are shown in each document's own currency."));

                    col.Item().Element(c => BuildTable(c,
                        headers: ["Doc #", "Adj. Date", "Supplier", "Vessel", "Reason", "Type", "Amount", "Curr."],
                        rows: report.Adjustments.Select(a => new string?[]
                        {
                            a.DocNumber,
                            a.AdjustmentDate.ToString("dd MMM yyyy"),
                            a.SupplierName ?? a.BuyerName ?? "-",
                            a.VesselName ?? "-",
                            string.IsNullOrWhiteSpace(a.Reason) ? "-" : a.Reason,
                            a.Amount >= 0 ? "Increase" : "Decrease",
                            Signed(a.Amount),
                            a.CurrencyCode,
                        }).ToList(),
                        rightAlignedColumns: [6],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [6] = (Az.Warning, Az.Success),
                        },
                        netColumnIndex: 6,
                        netValues: report.Adjustments.Select(a => a.Amount).ToList(),
                        columnWidths: [2.3f, 1.5f, 2.6f, 1.4f, 3.4f, 1.2f, 1.5f, 0.8f]
                    ));
                }

                // ── Documents register ────────────────────────────────────
                col.Item().Element(SectionTitle($"Documents ({report.Count})"));

                if (hasAdjustments)
                {
                    col.Item().Element(c => NoteLine(c,
                        "\"Net\" is the agreed price after adjustment and is the basis for Paid and Remaining."));

                    col.Item().Element(c => BuildTable(c,
                        headers: ["Doc #", "Date", "Type", "Supplier", "Vessel", "Curr.",
                                  "Original", "Adjust.", "Net", "Paid", "Remaining"],
                        rows: report.Documents.Select(d => new string?[]
                        {
                            d.DocNumber ?? "-",
                            d.DocDate.ToString("dd MMM yyyy"),
                            d.DocTypeName ?? "-",
                            d.SupplierName ?? d.BuyerName ?? "-",
                            d.VesselName ?? "-",
                            d.CurrencyCode ?? "-",
                            $"{d.TotalAmount:N2}",
                            d.AdjustmentsTotal == 0 ? "-" : Signed(d.AdjustmentsTotal),
                            $"{d.NetAmount:N2}",
                            $"{d.PaidAmount:N2}",
                            $"{d.Remaining:N2}",
                        }).ToList(),
                        rightAlignedColumns: [6, 7, 8, 9, 10],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [7] = (Az.Warning, Az.Success),
                            [9] = (Az.Success, Az.Success),
                            [10] = (Az.Warning, Az.Warning),
                        },
                        netColumnIndex: 7,
                        netValues: report.Documents.Select(d => d.AdjustmentsTotal).ToList(),
                        mutedColumns: [6],
                        columnWidths: [2.3f, 1.5f, 1.5f, 2.6f, 1.4f, 0.8f, 1.5f, 1.3f, 1.5f, 1.5f, 1.5f]
                    ));
                }
                else
                {
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Doc #", "Date", "Type", "Supplier", "Vessel", "Curr.",
                                  "Amount", "Paid", "Remaining", "Side"],
                        rows: report.Documents.Select(d => new string?[]
                        {
                            d.DocNumber ?? "-",
                            d.DocDate.ToString("dd MMM yyyy"),
                            d.DocTypeName ?? "-",
                            d.SupplierName ?? d.BuyerName ?? "-",
                            d.VesselName ?? "-",
                            d.CurrencyCode ?? "-",
                            $"{d.NetAmount:N2}",
                            $"{d.PaidAmount:N2}",
                            $"{d.Remaining:N2}",
                            d.Side.ToString(),
                        }).ToList(),
                        rightAlignedColumns: [6, 7, 8],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [7] = (Az.Success, Az.Success),
                            [8] = (Az.Warning, Az.Warning),
                        },
                        columnWidths: [2.3f, 1.5f, 1.6f, 3.0f, 1.6f, 0.9f, 1.6f, 1.6f, 1.6f, 1.2f]
                    ));
                }
            });
        };

        // ── Reconciliation block ───────────────────────────────────────────
        private static void ReconciliationBlock(IContainer container, DocumentReport report)
        {
            var count = report.Adjustments.Count;

            container
                .Border(1).BorderColor(Az.Line)
                .Background(Az.Surface)
                .Padding(12)
                .Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Original Invoiced Value")
                           .FontSize(9).FontColor(Az.TextMuted);
                        row.ConstantItem(140).AlignRight()
                           .Text($"{report.TotalOriginalValue:N2}").FontSize(9).SemiBold();
                    });

                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("Price Adjustments").FontSize(9).FontColor(Az.TextMuted);
                            t.Span($"   {count} {(count == 1 ? "entry" : "entries")}")
                             .FontSize(7.5f).FontColor(Az.TextFaint);
                        });
                        row.ConstantItem(140).AlignRight()
                           .Text(Signed(report.TotalAdjustments)).FontSize(9).SemiBold()
                           .FontColor(report.TotalAdjustments < 0 ? Az.Success : Az.Warning);
                    });

                    col.Item().PaddingTop(6).BorderTop(1).BorderColor(Az.LineStrong).PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text("Net Reported Value").FontSize(10).Bold();
                        row.ConstantItem(140).AlignRight()
                           .Text($"{report.TotalValue:N2}").FontSize(10).Bold().FontColor(Az.Primary);
                    });

                    col.Item().PaddingTop(8).Text(
                        "All summary figures in this report — totals, balances and breakdowns — are stated " +
                        "net of the adjustments listed at the end of this report.")
                       .FontSize(7.5f).FontColor(Az.TextMuted);
                });
        }

        // ═══════════════════════════════════════════════════════════════════
        //  PAYMENT REPORT  (logic unchanged — visual language aligned)
        // ═══════════════════════════════════════════════════════════════════
        public Task<byte[]> GeneratePaymentReportPdf(PaymentsReport report, string title, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            QuestPDF.Settings.License = LicenseType.Community;

            var baseCurrency = report.BaseCurrencyCode ?? "";

            var bytes = Document.Create(container =>
            {
                // ── Page 1 — Portrait: summaries ──────────────────────────
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.4f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Az.Font).FontColor(Az.Text));

                    page.Header().Element(ComposeHeader(title, baseCurrency));
                    page.Content().PaddingTop(10).Element(ComposePaymentSummaryContent(report));
                    page.Footer().Element(ComposeFooter("Payment Report", baseCurrency));
                });

                // ── Page 2 — Landscape: payment register ──────────────────
                if (report.Payments.Any())
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.4f, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Az.Font).FontColor(Az.Text));

                        page.Header().Element(ComposeHeader($"{title} — Payments", baseCurrency));
                        page.Content().PaddingTop(10).Element(ComposePaymentsContent(report));
                        page.Footer().Element(ComposeFooter("Payment Report", baseCurrency));
                    });
                }
            }).GeneratePdf();

            return Task.FromResult(bytes);
        }

        // ── Payment summary content (Portrait) ─────────────────────────────
        private static Action<IContainer> ComposePaymentSummaryContent(PaymentsReport report) => container =>
        {
            container.Column(col =>
            {
                col.Spacing(14);

                // ── KPI tiles ─────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.Spacing(8);
                    KpiTile(row.RelativeItem(), "Total Payments", report.Count.ToString(), null, Az.Primary);
                    KpiTile(row.RelativeItem(), "Cash In", $"{report.CashIn:N2}", null, Az.Success);
                    KpiTile(row.RelativeItem(), "Cash Out", $"{report.CashOut:N2}", null, Az.Error);
                    KpiTile(row.RelativeItem(), "Net Cash Flow", $"{report.NetCashFlow:N2}", "cash in minus cash out",
                            report.NetCashFlow >= 0 ? Az.Success : Az.Error);
                });

                // ── Monthly breakdown ─────────────────────────────────────
                if (report.MonthlySummary.Any())
                {
                    col.Item().Element(SectionTitle("Monthly Breakdown"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Year", "Month", "Count", "Cash In", "Cash Out", "Net"],
                        rows: report.MonthlySummary.Select(m => new string?[]
                        {
                            m.Year.ToString(),
                            System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m.Month),
                            m.Count.ToString(),
                            $"{m.CashIn:N2}",
                            $"{m.CashOut:N2}",
                            $"{m.NetCashFlow:N2}",
                        }).ToList(),
                        rightAlignedColumns: [2, 3, 4, 5],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Az.Success, Az.Success),
                            [4] = (Az.Error, Az.Error),
                            [5] = (Az.Success, Az.Error),
                        },
                        netColumnIndex: 5,
                        netValues: report.MonthlySummary.Select(m => m.NetCashFlow).ToList(),
                        columnWidths: [0.9f, 1.7f, 0.8f, 1.6f, 1.6f, 1.6f]
                    ));
                }

                // ── By method ─────────────────────────────────────────────
                if (report.MethodSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Method"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Method", "Count", "Cash In", "Cash Out", "Total"],
                        rows: report.MethodSummary.Select(m => new string?[]
                        {
                            m.Method,
                            m.Count.ToString(),
                            $"{m.CashIn:N2}",
                            $"{m.CashOut:N2}",
                            $"{m.TotalBase:N2}",
                        }).ToList(),
                        rightAlignedColumns: [1, 2, 3, 4],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [2] = (Az.Success, Az.Success),
                            [3] = (Az.Error, Az.Error),
                        },
                        columnWidths: [2.6f, 0.9f, 1.6f, 1.6f, 1.6f]
                    ));
                }

                // ── By vessel ─────────────────────────────────────────────
                if (report.VesselSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Vessel"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Vessel", "Count", "Cash In", "Cash Out", "Net"],
                        rows: report.VesselSummary.Select(v => new string?[]
                        {
                            v.VesselName,
                            v.Count.ToString(),
                            $"{v.CashIn:N2}",
                            $"{v.CashOut:N2}",
                            $"{v.NetCashFlow:N2}",
                        }).ToList(),
                        rightAlignedColumns: [1, 2, 3, 4],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [2] = (Az.Success, Az.Success),
                            [3] = (Az.Error, Az.Error),
                            [4] = (Az.Success, Az.Error),
                        },
                        netColumnIndex: 4,
                        netValues: report.VesselSummary.Select(v => v.NetCashFlow).ToList(),
                        columnWidths: [2.6f, 0.9f, 1.6f, 1.6f, 1.6f]
                    ));
                }

                // ── By supplier ───────────────────────────────────────────
                if (report.SupplierSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Supplier"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Supplier", "Count", "Total Paid"],
                        rows: report.SupplierSummary.Select(s => new string?[]
                        {
                            s.SupplierName,
                            s.Count.ToString(),
                            $"{s.TotalPaidBase:N2}",
                        }).ToList(),
                        rightAlignedColumns: [1, 2],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [2] = (Az.Error, Az.Error),
                        },
                        columnWidths: [3.8f, 0.9f, 1.7f]
                    ));
                }

                // ── By buyer ──────────────────────────────────────────────
                if (report.BuyerSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Buyer"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Buyer", "Count", "Total Received"],
                        rows: report.BuyerSummary.Select(b => new string?[]
                        {
                            b.BuyerName,
                            b.Count.ToString(),
                            $"{b.TotalReceivedBase:N2}",
                        }).ToList(),
                        rightAlignedColumns: [1, 2],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [2] = (Az.Success, Az.Success),
                        },
                        columnWidths: [3.8f, 0.9f, 1.7f]
                    ));
                }

                // ── By voyage ─────────────────────────────────────────────
                if (report.VoyageSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Voyage"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Voyage", "Count", "Cash In", "Cash Out", "Net"],
                        rows: report.VoyageSummary.Select(v => new string?[]
                        {
                            v.VoyageNumber,
                            v.Count.ToString(),
                            $"{v.CashIn:N2}",
                            $"{v.CashOut:N2}",
                            $"{v.NetCashFlow:N2}",
                        }).ToList(),
                        rightAlignedColumns: [1, 2, 3, 4],
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [2] = (Az.Success, Az.Success),
                            [3] = (Az.Error, Az.Error),
                            [4] = (Az.Success, Az.Error),
                        },
                        netColumnIndex: 4,
                        netValues: report.VoyageSummary.Select(v => v.NetCashFlow).ToList(),
                        columnWidths: [2.6f, 0.9f, 1.6f, 1.6f, 1.6f]
                    ));
                }
            });
        };

        // ── Payments content (Landscape) ───────────────────────────────────
        private static Action<IContainer> ComposePaymentsContent(PaymentsReport report) => container =>
        {
            container.Column(col =>
            {
                col.Spacing(14);

                col.Item().Element(SectionTitle($"Payments ({report.Count})"));
                col.Item().Element(c => NoteLine(c,
                    "\"Paid\" is in the document's own currency. \"Paid (Base)\" is converted for comparison."));

                col.Item().Element(c => BuildTable(c,
                    headers: ["Payment #", "Date", "Doc #", "Doc Type", "Party", "Vessel", "Voyage",
                              "Curr.", "Method", "Paid", "Paid (Base)", "Side"],
                    rows: report.Payments.Select(p => new string?[]
                    {
                        p.PaymentId.ToString(),
                        p.PaymentDate.ToString("dd MMM yyyy"),
                        p.DocNumber ?? "-",
                        p.DocTypeName ?? "-",
                        p.SupplierName ?? p.BuyerName ?? "-",
                        p.VesselName ?? "-",
                        p.VoyageNumber ?? "-",
                        p.CurrencyCode ?? "-",
                        p.PaymentMethod.ToString(),
                        $"{p.PaidAmount:N2}",
                        $"{p.PaidAmountBase:N2}",
                        p.Side.ToString(),
                    }).ToList(),
                    rightAlignedColumns: [9, 10],
                    coloredColumns: new Dictionary<int, (string positive, string negative)>
                    {
                        [10] = (Az.Success, Az.Success),
                    },
                    mutedColumns: [0],
                    columnWidths:
                    [
                        1.0f, // Payment #
                        1.5f, // Date
                        2.1f, // Doc #
                        1.4f, // Doc Type
                        2.4f, // Party
                        1.4f, // Vessel
                        1.2f, // Voyage
                        0.8f, // Curr.
                        1.3f, // Method
                        1.5f, // Paid
                        1.5f, // Paid (Base)
                        1.0f  // Side
                    ]
                ));
            });
        };

        // ═══════════════════════════════════════════════════════════════════
        //  BILL OF LADING  (logic unchanged — visual language aligned)
        // ═══════════════════════════════════════════════════════════════════
        public async Task<byte[]> GenerateBillOfLadingFile(int blId, CancellationToken ct = default)
        {
            var bl = await _billService.GetByIdAsync(blId, ct);
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.4f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily(Az.Font).FontColor(Az.Text));

                    page.Header().BorderBottom(2).BorderColor(Az.Primary).PaddingBottom(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("BILL OF LADING").FontSize(17).Bold().FontColor(Az.Primary);
                            c.Item().PaddingTop(1).Text($"B/L No. {bl.BlNumber}")
                                    .FontSize(10).FontColor(Az.Text);
                        });
                        row.ConstantItem(160).AlignRight().Column(c =>
                        {
                            c.Item().AlignRight().Text("ISSUE DATE")
                                    .FontSize(7).SemiBold().FontColor(Az.TextFaint);
                            c.Item().AlignRight().Text(bl.IssueDate?.ToString("dd MMM yyyy") ?? "—")
                                    .FontSize(10).SemiBold();
                        });
                    });

                    page.Content().PaddingTop(14).Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Element(SectionTitle("Parties"));

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Component(new FieldBlock("Shipper", bl.ShipperCompany?.Name, bold: true));
                            row.RelativeItem().Component(new FieldBlock("Carrier", bl.CarrierCompany?.Name, bold: true));
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Component(new FieldBlock("Consignee",
                                bl.ConsigneeCompany?.Name ?? (string.IsNullOrWhiteSpace(bl.ConsigneeToOrder)
                                    ? "—"
                                    : $"To Order: {bl.ConsigneeToOrder}")));
                            row.RelativeItem().Component(new FieldBlock("Notify Party", bl.NotifyPartyCompany?.Name ?? "—"));
                        });

                        col.Item().Element(SectionTitle("Voyage & Routing"));

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Component(new FieldBlock("Port of Loading", bl.PortOfLoading?.Name));
                            row.RelativeItem().Component(new FieldBlock("Port of Discharge", bl.PortOfDischarge?.Name));
                        });
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Component(new FieldBlock("Place of Receipt", bl.PlaceOfReceipt?.Name ?? "—"));
                            row.RelativeItem().Component(new FieldBlock("Place of Delivery", bl.PlaceOfDelivery?.Name ?? "—"));
                        });

                        if (bl.IssuanceType == BlIssuanceType.House && bl.MasterBlId is not null)
                        {
                            col.Item()
                               .Border(1).BorderColor(Az.Line)
                               .BorderLeft(3).BorderColor(Az.Primary)
                               .Background(Az.RowAlt)
                               .PaddingVertical(7).PaddingHorizontal(9)
                               .Text($"House B/L linked to Master B/L: {bl.BlNumber}")
                               .FontSize(9).FontColor(Az.Text);
                        }

                        col.Item().Element(SectionTitle("Cargo Details"));

                        col.Item().Component(new FieldBlock("Description of Goods", bl.CargoDescription));
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Component(new FieldBlock("HS Code", bl.HsCode ?? "—"));
                            row.RelativeItem().Component(new FieldBlock("Gross Weight", $"{bl.GrossWeightMT:N3} MT"));
                            row.RelativeItem().Component(new FieldBlock("Volume",
                                bl.VolumeM3.HasValue ? $"{bl.VolumeM3:N3} M³" : "—"));
                        });
                        col.Item().Component(new FieldBlock("Marks & Numbers", bl.MarksAndNumbers ?? "—"));

                        col.Item().Element(SectionTitle("Freight & Terms"));

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Component(new FieldBlock("Freight Terms", bl.FreightTerms));
                            row.RelativeItem().Component(new FieldBlock("Freight Amount", bl.FreightAmount ?? "—"));
                            row.RelativeItem().Component(new FieldBlock("Incoterms", bl.Incoterms ?? "—"));
                        });

                        if (!string.IsNullOrWhiteSpace(bl.Notes))
                        {
                            col.Item().Element(SectionTitle("Notes"));
                            col.Item().Text(bl.Notes).FontSize(9).FontColor(Az.Text);
                        }
                    });

                    page.Footer().BorderTop(1).BorderColor(Az.Line).PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text($"Marilog · Bill of Lading · {bl.BlNumber}")
                           .FontSize(7).FontColor(Az.TextFaint);

                        row.ConstantItem(150).AlignRight()
                           .Text($"Generated {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                           .FontSize(7).FontColor(Az.TextFaint);
                    });
                });
            });

            return document.GeneratePdf();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  SHARED BUILDING BLOCKS
        // ═══════════════════════════════════════════════════════════════════

        // ── Page header ────────────────────────────────────────────────────
        private static Action<IContainer> ComposeHeader(string title, string baseCurrencyCode) => container =>
        {
            container.BorderBottom(2).BorderColor(Az.Primary).PaddingBottom(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("MARILOG").FontSize(17).Bold().FontColor(Az.Primary);
                    col.Item().PaddingTop(1).Text(title).FontSize(11).FontColor(Az.Text);
                });

                row.ConstantItem(200).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text(t =>
                    {
                        t.Span("BASE CURRENCY   ").FontSize(7).FontColor(Az.TextFaint);
                        t.Span(baseCurrencyCode).FontSize(9).SemiBold().FontColor(Az.Text);
                    });
                    col.Item().PaddingTop(2).AlignRight()
                       .Text($"Generated {DateTime.Now:dd MMM yyyy HH:mm}")
                       .FontSize(7.5f).FontColor(Az.TextMuted);
                });
            });
        };

        // ── Page footer ────────────────────────────────────────────────────
        private static Action<IContainer> ComposeFooter(string reportName, string baseCurrencyCode) => container =>
        {
            container.BorderTop(1).BorderColor(Az.Line).PaddingTop(5).Row(row =>
            {
                row.RelativeItem()
                   .Text($"Marilog · {reportName} · Amounts in {baseCurrencyCode} unless stated otherwise")
                   .FontSize(7).FontColor(Az.TextFaint);

                row.ConstantItem(90).AlignRight().Text(x =>
                {
                    x.DefaultTextStyle(s => s.FontSize(7).FontColor(Az.TextFaint));
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        };

        // ── KPI tile ───────────────────────────────────────────────────────
        private static void KpiTile(IContainer container, string label, string value,
                                    string? caption, string accent)
        {
            container
                .Border(1).BorderColor(Az.Line)
                .BorderLeft(3).BorderColor(accent)
                .Background(Az.Surface)
                .PaddingVertical(9).PaddingHorizontal(10)
                .Column(col =>
                {
                    col.Item().Text(label.ToUpperInvariant())
                       .FontSize(6.8f).SemiBold().FontColor(Az.TextMuted);

                    col.Item().PaddingTop(3).Text(value)
                       .FontSize(13).Bold().FontColor(Az.Text);

                    if (!string.IsNullOrWhiteSpace(caption))
                    {
                        col.Item().PaddingTop(2).Text(caption)
                           .FontSize(6.8f).FontColor(Az.TextFaint);
                    }
                });
        }

        // ── Section title ──────────────────────────────────────────────────
        private static Action<IContainer> SectionTitle(string title) => container =>
        {
            container.PaddingBottom(4).Text(title.ToUpperInvariant())
                .FontSize(8).Bold().FontColor(Az.TextMuted);
        };

        // ── Explanatory note under a section title ─────────────────────────
        private static void NoteLine(IContainer container, string text)
        {
            container.PaddingBottom(5).Text(text).FontSize(7.5f).FontColor(Az.TextMuted);
        }

        // ── Signed money formatting ────────────────────────────────────────
        private static string Signed(decimal value)
            => value > 0 ? $"+{value:N2}" : value.ToString("N2");

        // ── Table builder ──────────────────────────────────────────────────
        private static void BuildTable(
            IContainer container,
            string[] headers,
            List<string?[]> rows,
            Dictionary<int, (string positive, string negative)>? coloredColumns = null,
            int? netColumnIndex = null,
            List<decimal>? netValues = null,
            float[]? columnWidths = null,
            int[]? rightAlignedColumns = null,
            int[]? mutedColumns = null)
        {
            container.Border(1).BorderColor(Az.Line).Table(table =>
            {
                // ── Column widths ─────────────────────────────────────────
                table.ColumnsDefinition(cols =>
                {
                    if (columnWidths != null)
                        foreach (var w in columnWidths)
                            cols.RelativeColumn(w);
                    else
                        for (int i = 0; i < headers.Length; i++)
                            cols.RelativeColumn();
                });

                // ── Header ────────────────────────────────────────────────
                table.Header(headerRow =>
                {
                    for (int colIdx = 0; colIdx < headers.Length; colIdx++)
                    {
                        var header = headers[colIdx];
                        var alignRight = IsRightAligned(colIdx, header, rightAlignedColumns);

                        var cell = headerRow.Cell()
                            .Background(Az.HeaderBg)
                            .BorderBottom(1).BorderColor(Az.LineStrong)
                            .PaddingVertical(5).PaddingHorizontal(5);

                        (alignRight ? cell.AlignRight() : cell.AlignLeft())
                            .Text(header.ToUpperInvariant())
                            .FontSize(6.8f).SemiBold().FontColor(Az.TextMuted);
                    }
                });

                // ── Rows ──────────────────────────────────────────────────
                for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
                {
                    var row = rows[rowIdx];
                    var bg = rowIdx % 2 == 0 ? Az.Surface : Az.RowAlt;

                    for (int colIdx = 0; colIdx < row.Length; colIdx++)
                    {
                        var cellText = row[colIdx] ?? "-";
                        var textColor = Az.Text;

                        if (mutedColumns?.Contains(colIdx) == true)
                            textColor = Az.TextMuted;

                        if (coloredColumns != null && coloredColumns.TryGetValue(colIdx, out var colors))
                        {
                            if (netColumnIndex.HasValue && colIdx == netColumnIndex.Value && netValues != null)
                                textColor = netValues[rowIdx] >= 0 ? colors.positive : colors.negative;
                            else
                                textColor = colors.positive;
                        }

                        var alignRight = IsRightAligned(colIdx, headers[colIdx], rightAlignedColumns);

                        var cell = table.Cell()
                            .Background(bg)
                            .BorderBottom(1).BorderColor(Az.Line)
                            .PaddingVertical(4).PaddingHorizontal(5);

                        (alignRight ? cell.AlignRight() : cell.AlignLeft())
                            .Text(cellText)
                            .FontSize(7.8f).FontColor(textColor);
                    }
                }
            });
        }

        // Explicit index list wins; otherwise fall back to known numeric headers.
        private static bool IsRightAligned(int colIdx, string header, int[]? rightAlignedColumns)
        {
            if (rightAlignedColumns != null)
                return rightAlignedColumns.Contains(colIdx);

            return header is "Count" or "Value" or "Paid" or "Remaining" or "Revenue"
                or "Expense" or "Net" or "Amount" or "Cash In" or "Cash Out" or "Total";
        }

        // ── Azure-inspired palette ─────────────────────────────────────────
        private static class Az
        {
            public const string Font = "Segoe UI";

            public const string Primary = "#0F6CBD";
            public const string Success = "#107C10";
            public const string Warning = "#8A6100";
            public const string Error = "#A4262C";

            public const string Text = "#242424";
            public const string TextMuted = "#616161";
            public const string TextFaint = "#8A8886";

            public const string Surface = "#FFFFFF";
            public const string RowAlt = "#FAFAFA";
            public const string HeaderBg = "#F3F2F1";
            public const string Line = "#E1DFDD";
            public const string LineStrong = "#C8C6C4";
        }

        // ── Label/value block used by the Bill of Lading ───────────────────
        private sealed class FieldBlock(string label, string? value, bool bold = false) : IComponent
        {
            public void Compose(IContainer container)
            {
                container.Column(col =>
                {
                    col.Item().Text(label.ToUpperInvariant())
                       .FontSize(6.8f).SemiBold().FontColor(Az.TextMuted);

                    var text = col.Item().Text(string.IsNullOrWhiteSpace(value) ? "—" : value)
                                  .FontSize(10).FontColor(Az.Text);

                    if (bold)
                        text.Bold();
                });
            }
        }
    }
}