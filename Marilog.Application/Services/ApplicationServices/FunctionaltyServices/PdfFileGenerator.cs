using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.Interfaces.Services.FunctionaltyServices;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Services.ApplicationServices.FunctionaltyServices
{
    public class PdfFileGenerator : IPdfFileGenerator
    {
        public byte[] GenerateDocumentReportPdf(DocumentReport report, string title)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader(title));
                    page.Content().Element(ComposeContent(report));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }


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
                    col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
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
                    row.Spacing(8);
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
                        row.Spacing(8);
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
                    col.Item().Element(container => BuildTable(container,
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
                            [8] = (Colors.Green.Darken1, Colors.Red.Darken1),   // Net — يتغير بالقيمة
                        },
                        netColumnIndex: 8,
                        netValues: report.MonthlySummary.Select(m => m.NetPosition).ToList()
                    ));
                }

                // ── By Supplier ────────────────────────────────────────────
                if (report.SupplierSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Supplier"));
                    col.Item().Element(container => BuildTable(container,
                        headers: ["Supplier", "Count", "Value", "Paid", "Remaining"],
                        rows: report.SupplierSummary.Select(s => new[]
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
                        }
                    ));
                }

                // ── By Vessel ──────────────────────────────────────────────
                if (report.VesselSummary.Any())
                {
                    col.Item().Element(SectionTitle("By Vessel"));
                    col.Item().Element(container => BuildTable(container,
                        headers: ["Vessel", "Count", "Value", "Paid", "Remaining", "Revenue", "Expense", "Net"],
                        rows: report.VesselSummary.Select(v => new[]
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
                        netValues: report.VesselSummary.Select(v => v.NetPosition).ToList()
                    ));
                }

                // ── Documents ──────────────────────────────────────────────
                if (report.Documents.Any())
                {
                    col.Item().Element(SectionTitle($"Documents ({report.Count})"));
                    col.Item().Element(container => BuildTable(container,
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
                        }
                    ));
                }
            });
        };

        // ── Helpers ───────────────────────────────────────────────────────────
        private static void KpiCard(IContainer container, string label, string value, string bgColor, string textColor)
        {
            container.Background(bgColor).Padding(10).Column(col =>
            {
                col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                col.Item().Text(value).FontSize(13).Bold().FontColor(textColor);
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
    List<string?[]> rows,                    // ← string?[] بدل string[]
    Dictionary<int, (string positive, string negative)>? coloredColumns = null,
    int? netColumnIndex = null,
    List<decimal>? netValues = null)
        {
            container.Table(table =>
            {
                // Columns
                table.ColumnsDefinition(cols =>
                {
                    for (int i = 0; i < headers.Length; i++)
                        cols.RelativeColumn();
                });

                // Header
                foreach (var header in headers)
                {
                    table.Header(header =>
                    {
                        foreach (var h in headers)
                        {
                            header.Cell()
                                .Background(Colors.Blue.Darken2)
                                .Padding(5)
                                .Text(h)
                                .FontSize(8)
                                .Bold()
                                .FontColor(Colors.White);
                        }
                    });
                }

                // Rows
                for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
                {
                    var row = rows[rowIdx];
                    var bg = rowIdx % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    for (int colIdx = 0; colIdx < row.Length; colIdx++)
                    {
                        var cellText = row[colIdx];
                        var textColor = Colors.Black;

                        if (coloredColumns != null && coloredColumns.TryGetValue(colIdx, out var colors))
                        {
                            // Net column — اللون يعتمد على القيمة الفعلية
                            if (netColumnIndex.HasValue && colIdx == netColumnIndex.Value && netValues != null)
                                textColor = netValues[rowIdx] >= 0 ? colors.positive : colors.negative;
                            else
                                textColor = colors.positive;
                        }

                        var isNumeric = colIdx > 0 && decimal.TryParse(cellText?.Replace(",", ""), out _);

                        table.Cell()
                            .Background(bg)
                            .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(4)
                            .AlignRight()
                            .Text(cellText).FontSize(8).FontColor(textColor);
                    }
                }
            });
        }
    }
}
