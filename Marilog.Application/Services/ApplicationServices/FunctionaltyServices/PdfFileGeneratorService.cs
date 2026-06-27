using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.Interfaces.Services.FunctionaltyServices;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Marilog.Application.Services.ApplicationServices.FunctionaltyServices
{
    public class PdfFileGeneratorService : IPdfFileGeneratorService
    {
        public Task<byte[]> GenerateDocumentReportPdf(DocumentReport report, string title, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            QuestPDF.Settings.License = LicenseType.Community;

            var bytes = Document.Create(container =>
            {
                // ── صفحة Portrait للملخصات ────────────────────────────────
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader(title));
                    page.Content().Element(ComposeSummaryContent(report));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });

                // ── صفحة Landscape للمستندات ──────────────────────────────
                if (report.Documents.Any())
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.5f, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                        page.Header().Element(ComposeHeader($"{title} — Documents"));
                        page.Content().Element(ComposeDocumentsContent(report));
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                            x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                            x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
                }
            }).GeneratePdf();

            return Task.FromResult(bytes);
        }

        // ── Summary Content (Portrait) ────────────────────────────────────────────
        private static Action<IContainer> ComposeSummaryContent(DocumentReport report) => container =>
        {
            container.Column(col =>
            {
                col.Spacing(12);

                // KPI Cards
                col.Item().Row(row =>
                {
                    row.Spacing(6);
                    KpiCard(row.RelativeItem(), "Total Docs", report.Count.ToString(), Colors.Blue.Lighten4, Colors.Blue.Darken2);
                    KpiCard(row.RelativeItem(), "Total Value", $"{report.TotalValue:N2}", Colors.Grey.Lighten3, Colors.Grey.Darken3);
                    KpiCard(row.RelativeItem(), "Total Paid", $"{report.TotalPaid:N2}", Colors.Green.Lighten4, Colors.Green.Darken2);
                    KpiCard(row.RelativeItem(), "Remaining", $"{report.TotalRemaining:N2}", Colors.Red.Lighten4, Colors.Red.Darken2);
                });

                // Financial Overview
                if (report.RevenueSideSummary?.Any() == true || report.ExpenseSideSummary?.Any() == true)
                {
                    var revenue = report.RevenueSideSummary?.Sum(x => x.TotalValue) ?? 0m;
                    var expense = report.ExpenseSideSummary?.Sum(x => x.TotalValue) ?? 0m;
                    var net = report.NetPosition;

                    col.Item().Element(SectionTitle("Financial Overview"));
                    col.Item().Row(row =>
                    {
                        row.Spacing(6);
                        KpiCard(row.RelativeItem(), "Revenue", $"{revenue:N2}", Colors.Green.Lighten4, Colors.Green.Darken2);
                        KpiCard(row.RelativeItem(), "Expense", $"{expense:N2}", Colors.Red.Lighten4, Colors.Red.Darken2);
                        KpiCard(row.RelativeItem(), "Net Position", $"{net:N2}",
                            net >= 0 ? Colors.Green.Lighten4 : Colors.Red.Lighten4,
                            net >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                    });
                }

                // Monthly Breakdown
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
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [4] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [5] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [6] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [7] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [8] = (Colors.Green.Darken1, Colors.Red.Darken1),
                        },
                        netColumnIndex: 8,
                        netValues: report.MonthlySummary.Select(m => m.NetPosition).ToList(),
                        columnWidths:
                                    [
                                        0.8f, // Year
                                        1.6f, // Month
                                        0.8f, // Count
                                        1.4f, // Value
                                        1.4f, // Paid
                                        1.4f, // Remaining
                                        1.4f, // Revenue
                                        1.4f, // Expense
                                        1.4f  // Net
                                    ]
                    ));
                }

                // By Supplier
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
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [4] = (Colors.Red.Darken1, Colors.Red.Darken1),
                        },
                        columnWidths:
                            [
                                3.6f, // Supplier
                                0.8f, // Count
                                1.4f, // Value
                                1.4f, // Paid
                                1.4f  // Remaining
                            ]
                    ));
                }

                // By Vessel
                if (report.VesselSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Vessel"));
                    col.Item().Element
                    (c => BuildTable
                    (c,
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
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [4] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [5] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [6] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [7] = (Colors.Green.Darken1, Colors.Red.Darken1),
                        },
                        netColumnIndex: 7,
                        netValues: report.VesselSummary.Select(v => v.NetPosition).ToList(),
                        columnWidths:
                        [
                                2.0f, // Vessel
                                0.8f, // Count
                                1.4f, // Value
                                1.4f, // Paid
                                1.4f, // Remaining
                                1.4f, // Revenue
                                1.4f, // Expense
                                1.4f  // Net
                        ]
                    )
                    );
                }


                // By Voyage
                if (report.VoyageSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Voyage"));
                    col.Item().Element
                    (c => BuildTable
                    (c,
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
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [4] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [5] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [6] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [7] = (Colors.Green.Darken1, Colors.Red.Darken1),
                        },
                        netColumnIndex: 7,
                        netValues: report.VoyageSummary.Select(v => v.NetPosition).ToList(),
                        columnWidths:
                        [
                                2.0f, // Voyage
                 0.8f, // Count
                 1.4f, // Value
                 1.4f, // Paid
                 1.4f, // Remaining
                 1.4f, // Revenue
                 1.4f, // Expense
                 1.4f  // Net
                        ]
                    )
                    );
                }

            });
        };

        // ── Documents Content (Landscape) ─────────────────────────────────────────
        private static Action<IContainer> ComposeDocumentsContent(DocumentReport report) => container =>
        {
            container.Column(col =>
            {
                col.Spacing(12);
                col.Item().Element(SectionTitle($"Documents ({report.Count})"));
                col.Item().Element(c => BuildTable(c,
                    headers: ["Doc #", "Date", "Type", "Supplier", "Vessel", "Currency", "Amount", "Paid", "Remaining", "Side"],
                    rows: report.Documents.Select(d => new string?[]
                    {
                d.DocNumber ?? "-",
                d.DocDate.ToString("yyyy-MM-dd"),
                d.DocTypeName ?? "-",
                d.SupplierName ?? d.BuyerName ?? "-",
                d.VesselName ?? "-",
                d.CurrencyCode ?? "-",
                $"{d.TotalAmount:N2}",
                $"{d.PaidAmount:N2}",
                $"{d.Remaining:N2}",
                d.Side.ToString(),
                    }).ToList(),
                    coloredColumns: new Dictionary<int, (string positive, string negative)>
                    {
                        [7] = (Colors.Green.Darken1, Colors.Green.Darken1),
                        [8] = (Colors.Red.Darken1, Colors.Red.Darken1),
                    },
                    columnWidths:
[
    1.2f, // Doc #
    1.2f, // Date
    1.3f, // Type
    2.8f, // Supplier
    1.5f, // Vessel
    1.0f, // Currency
    1.4f, // Amount
    1.4f, // Paid
    1.4f, // Remaining
    1.0f  // Side
]
                ));
            });
        };

        // ── Header ────────────────────────────────────────────────────────────
        private static Action<IContainer> ComposeHeader(string title) => container =>
        {
            container.BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("MARILOG").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text(title).FontSize(11).FontColor(Colors.Grey.Darken2);
                });
                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        };

        // ── Content ───────────────────────────────────────────────────────────
        private static Action<IContainer> ComposeContent(DocumentReport report) => container =>
        {
            container.Column(col =>
            {
                col.Spacing(12);

                // ── KPI Cards ─────────────────────────────────────────────
                col.Item().Row(row =>
                {
                    row.Spacing(6);
                    KpiCard(row.RelativeItem(), "Total Docs", report.Count.ToString(), Colors.Blue.Lighten4, Colors.Blue.Darken2);
                    KpiCard(row.RelativeItem(), "Total Value", $"{report.TotalValue:N2}", Colors.Grey.Lighten3, Colors.Grey.Darken3);
                    KpiCard(row.RelativeItem(), "Total Paid", $"{report.TotalPaid:N2}", Colors.Green.Lighten4, Colors.Green.Darken2);
                    KpiCard(row.RelativeItem(), "Remaining", $"{report.TotalRemaining:N2}", Colors.Red.Lighten4, Colors.Red.Darken2);
                });

                // ── Financial Overview ─────────────────────────────────────
                if (report.RevenueSideSummary?.Any() == true || report.ExpenseSideSummary?.Any() == true)
                {
                    var revenue = report.RevenueSideSummary?.Sum(x => x.TotalValue) ?? 0m;
                    var expense = report.ExpenseSideSummary?.Sum(x => x.TotalValue) ?? 0m;
                    var net = report.NetPosition;

                    col.Item().Element(SectionTitle("Financial Overview"));
                    col.Item().Row(row =>
                    {
                        row.Spacing(6);
                        KpiCard(row.RelativeItem(), "Revenue", $"{revenue:N2}", Colors.Green.Lighten4, Colors.Green.Darken2);
                        KpiCard(row.RelativeItem(), "Expense", $"{expense:N2}", Colors.Red.Lighten4, Colors.Red.Darken2);
                        KpiCard(row.RelativeItem(), "Net Position", $"{net:N2}",
                            net >= 0 ? Colors.Green.Lighten4 : Colors.Red.Lighten4,
                            net >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                    });
                }

                // ── Monthly Breakdown ──────────────────────────────────────
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
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [4] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [5] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [6] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [7] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [8] = (Colors.Green.Darken1, Colors.Red.Darken1),
                        },
                        netColumnIndex: 8,
                        netValues: report.MonthlySummary.Select(m => m.NetPosition).ToList(),
                        columnWidths: [1f, 1.5f, 0.8f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f]
                    ));
                }

                // ── By Supplier ────────────────────────────────────────────
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
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [4] = (Colors.Red.Darken1, Colors.Red.Darken1),
                        },
                        columnWidths: [3f, 0.8f, 1.5f, 1.5f, 1.5f]
                    ));
                }

                // ── By Vessel ──────────────────────────────────────────────
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
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [3] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [4] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [5] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [6] = (Colors.Red.Darken1, Colors.Red.Darken1),
                            [7] = (Colors.Green.Darken1, Colors.Red.Darken1),
                        },
                        netColumnIndex: 7,
                        netValues: report.VesselSummary.Select(v => v.NetPosition).ToList(),
                        columnWidths: [2f, 0.8f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f]
                    ));
                }

                // ── Documents ──────────────────────────────────────────────
                if (report.Documents.Any())
                {
                    col.Item().Element(SectionTitle($"Documents ({report.Count})"));
                    col.Item().Element(c => BuildTable(c,
                        headers: ["Doc #", "Date", "Type", "Supplier", "Vessel", "Currency", "Amount", "Paid", "Remaining", "Side"],
                        rows: report.Documents.Select(d => new string?[]
                        {
                            d.DocNumber ?? "-",
                            d.DocDate.ToString("yyyy-MM-dd"),
                            d.DocTypeName ?? "-",
                            d.SupplierName ?? d.BuyerName ?? "-",
                            d.VesselName ?? "-",
                            d.CurrencyCode ?? "-",
                            $"{d.TotalAmount:N2}",
                            $"{d.PaidAmount:N2}",
                            $"{d.Remaining:N2}",
                            d.Side.ToString(),
                        }).ToList(),
                        coloredColumns: new Dictionary<int, (string positive, string negative)>
                        {
                            [7] = (Colors.Green.Darken1, Colors.Green.Darken1),
                            [8] = (Colors.Red.Darken1, Colors.Red.Darken1),
                        },
                        columnWidths: [1.2f, 1.2f, 1f, 2f, 1.2f, 1f, 1.5f, 1.5f, 1.5f, 1f]
                    ));
                }
            });
        };

        // ── Helpers ───────────────────────────────────────────────────────────
        private static void KpiCard(IContainer container, string label, string value,
                                    string bgColor, string textColor)
        {
            container.Background(bgColor).Padding(10).Column(col =>
            {
                col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                col.Item().Text(value).FontSize(12).Bold().FontColor(textColor);
            });
        }

        private static Action<IContainer> SectionTitle(string title) => container =>
        {
            container.BorderBottom(1).BorderColor(Colors.Blue.Lighten2).PaddingBottom(4)
                .Text(title).FontSize(10).Bold().FontColor(Colors.Blue.Darken2);
        };

        private static void BuildTable(
            IContainer container,
            string[] headers,
            List<string?[]> rows,
            Dictionary<int, (string positive, string negative)>? coloredColumns = null,
            int? netColumnIndex = null,
            List<decimal>? netValues = null,
            float[]? columnWidths = null)
        {
            container.Table(table =>
            {
                // ── Column Widths ─────────────────────────────────────────
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

                        var isNumericHeader =
                            header is "Count" or
                            "Value" or
                            "Paid" or
                            "Remaining" or
                            "Revenue" or
                            "Expense" or
                            "Net" or
                            "Amount";

                        var cell = headerRow.Cell()
                            .Background(Colors.Blue.Darken2)
                            .PaddingVertical(5)
                            .PaddingHorizontal(5);

                        if (isNumericHeader)
                        {
                            cell.AlignRight()
                                .Text(header)
                                .FontSize(8)
                                .Bold()
                                .FontColor(Colors.White);
                        }
                        else
                        {
                            cell.AlignLeft()
                                .Text(header)
                                .FontSize(8)
                                .Bold()
                                .FontColor(Colors.White);
                        }
                    }
                });

                // ── Rows ──────────────────────────────────────────────────
                for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
                {
                    var row = rows[rowIdx];
                    var bg = rowIdx % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    for (int colIdx = 0; colIdx < row.Length; colIdx++)
                    {
                        var cellText = row[colIdx] ?? "-";
                        var textColor = Colors.Black;

                        if (coloredColumns != null && coloredColumns.TryGetValue(colIdx, out var colors))
                        {
                            if (netColumnIndex.HasValue && colIdx == netColumnIndex.Value && netValues != null)
                                textColor = netValues[rowIdx] >= 0 ? colors.positive : colors.negative;
                            else
                                textColor = colors.positive;
                        }

                        var isNumeric = colIdx > 0 && decimal.TryParse(
                            cellText.Replace(",", "").Replace("-", ""), out _);

                        var cell = table.Cell()
                            .Background(bg)
                            .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .PaddingVertical(4).PaddingHorizontal(5);

                        if (isNumeric)
                            cell.AlignRight().Text(cellText).FontSize(8).FontColor(textColor);
                        else
                            cell.AlignLeft().Text(cellText).FontSize(8).FontColor(textColor);
                    }
                }
            });
        }
    }
}