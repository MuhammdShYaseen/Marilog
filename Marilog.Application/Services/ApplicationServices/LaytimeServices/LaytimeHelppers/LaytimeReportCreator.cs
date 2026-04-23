
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Marilog.Application.Interfaces.Services;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.ValueObjects.Laytime;
using Marilog.Kernel.Enums;
using System.Drawing;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeHelppers
{
    public class LaytimeReportCreator : ILaytimeReportCreator
    {
       
        // ═══════════════════════════════════════════════════════════════════
        // Time Sheet Excel
        // ═══════════════════════════════════════════════════════════════════
 
        public Task<byte[]> GenerateTimeSheetExcelAsync(
            LaytimeCalculation calculation,
            CharterTerms charterTerms,
            CancellationToken cancellationToken = default)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("TIME SHEET");

            BuildTimeSheet(sheet, calculation, charterTerms);

            return Task.FromResult(ToBytes(workbook));
        }

        // ═══════════════════════════════════════════════════════════════════
        // Summary Report
        // ═══════════════════════════════════════════════════════════════════

        public Task<byte[]> GenerateSummaryReportAsync(
            LaytimeCalculation calculation,
            CharterTerms charterTerms,
            ReportFormat format,
            CancellationToken cancellationToken = default)
        {
            if (format == ReportFormat.Pdf)
                return Task.FromResult(GenerateSummaryPdf(calculation, charterTerms));

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Summary");

            BuildSummarySheet(sheet, calculation, charterTerms);

            return Task.FromResult(ToBytes(workbook));
        }

        // ═══════════════════════════════════════════════════════════════════
        // Detailed Report
        // ═══════════════════════════════════════════════════════════════════

        public Task<byte[]> GenerateDetailedReportAsync(
            LaytimeCalculation calculation,
            CharterTerms charterTerms,
            ReportFormat format,
            CancellationToken cancellationToken = default)
        {
            if (format == ReportFormat.Pdf)
                return Task.FromResult(GenerateDetailedPdf(calculation, charterTerms));

            using var workbook = new XLWorkbook();

            BuildTimeSheet(workbook.Worksheets.Add("TIME SHEET"), calculation, charterTerms);
            BuildSofEventsSheet(workbook.Worksheets.Add("SOF Events"), calculation);
            BuildExceptionsSheet(workbook.Worksheets.Add("Exceptions"), calculation);
            BuildSummarySheet(workbook.Worksheets.Add("Summary"), calculation, charterTerms);

            return Task.FromResult(ToBytes(workbook));
        }

        // ═══════════════════════════════════════════════════════════════════
        // Delay Report
        // ═══════════════════════════════════════════════════════════════════

        public Task<byte[]> GenerateDelayReportAsync(
            LaytimeCalculation calculation,
            CharterTerms charterTerms,
            ReportFormat format,
            CancellationToken cancellationToken = default)
        {
            if (format == ReportFormat.Pdf)
                return Task.FromResult(GenerateDelayPdf(calculation, charterTerms));

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Delays");

            BuildDelaySheet(sheet, calculation, charterTerms);

            return Task.FromResult(ToBytes(workbook));
        }

        // ═══════════════════════════════════════════════════════════════════
        // Contract Laytime Report
        // ═══════════════════════════════════════════════════════════════════

        public Task<byte[]> GenerateContractLaytimeReportAsync(
            IReadOnlyList<LaytimeCalculation> calculations,
            CharterTerms charterTerms,
            ReportFormat format,
            CancellationToken cancellationToken = default)
        {
            if (format == ReportFormat.Pdf)
                return Task.FromResult(GenerateContractPdf(calculations, charterTerms));

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Contract Overview");

            BuildContractOverviewSheet(sheet, calculations, charterTerms);

            foreach (var calc in calculations)
            {
                var label = $"{calc.OperationType} #{calc.Id}";
                BuildTimeSheet(workbook.Worksheets.Add(label), calc, charterTerms);
            }

            return Task.FromResult(ToBytes(workbook));
        }

        // ═══════════════════════════════════════════════════════════════════
        // Sheet Builders
        // ═══════════════════════════════════════════════════════════════════

        private static void BuildTimeSheet(
            IXLWorksheet ws,
            LaytimeCalculation calc,
            CharterTerms charterTerms)
        {
            var result = calc.Result;
            var operationTerms = GetOperationTerms(charterTerms, calc.OperationType);
            var segments = calc.Segments.OrderBy(s => s.From).ToList();

            // ── Column widths ─────────────────────────────────────────────
            ws.Column(1).Width = 18; // DATE
            ws.Column(2).Width = 10; // DAY
            ws.Column(3).Width = 10; // FROM
            ws.Column(4).Width = 10; // TO
            ws.Column(5).Width = 8;  // D
            ws.Column(6).Width = 8;  // H
            ws.Column(7).Width = 8;  // M
            ws.Column(8).Width = 8;  // D (balance)
            ws.Column(9).Width = 8;  // H
            ws.Column(10).Width = 8; // M
            ws.Column(11).Width = 35; // REMARKS

            int row = 1;

            // ── Header Block ──────────────────────────────────────────────
            StyleHeader(ws.Cell(row, 1), "TIME SHEET");
            ws.Range(row, 1, row, 11).Merge().Style
              .Font.SetBold(true)
              .Font.SetFontSize(14)
              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
              .Fill.SetBackgroundColor(XLColor.DarkBlue);
            ws.Range(row, 1, row, 11).Style.Font.SetFontColor(XLColor.White);
            row++;

            // ── Vessel Info ───────────────────────────────────────────────
            row = WriteInfoRow(ws, row, "VESSEL NAME", $"MV — VoyageId: {calc.VoyageId}");
            row = WriteInfoRow(ws, row, "OPERATION", calc.OperationType.ToString().ToUpper());
            row = WriteInfoRow(ws, row, "PORT", $"Port #{calc.PortId}");
            row = WriteInfoRow(ws, row, "CARGO QTY", $"{calc.CargoQuantityMt:N0} MT");

            if (operationTerms is not null)
            {
                row = WriteInfoRow(ws, row, "L/D RATE",
                    $"{operationTerms.RateMtPerDay:N0} MT/DAY  {operationTerms.CalendarType}");
                row = WriteInfoRow(ws, row, "NOTICE HOURS",
                    $"{operationTerms.NoticeHours} HRS");
            }

            row = WriteInfoRow(ws, row, "DEM. RATE",
                $"{charterTerms.LaytimeTerms.Demurrage.RateUsdPerDay:N0} USD/DAY");

            if (charterTerms.LaytimeTerms.Despatch is not null)
                row = WriteInfoRow(ws, row, "DES. RATE",
                    $"{charterTerms.LaytimeTerms.Despatch.RateUsdPerDay:N0} USD/DAY");

            if (calc.LaytimeCommencedAt.HasValue)
                row = WriteInfoRow(ws, row, "TIME TO COUNT",
                    calc.LaytimeCommencedAt.Value.ToString("dd.MM.yyyy HH:mm"));
            row++;

            // ── Laytime Summary Block ─────────────────────────────────────
            if (result is not null)
            {
                row = WriteAllowedUsed(ws, row, result);
                row++;
            }

            // ── Table Header ──────────────────────────────────────────────
            WriteTableHeader(ws, row);
            row++;

            // ── Laytime Allowed Row ───────────────────────────────────────
            if (result is not null)
            {
                var allowedTs = TimeSpan.FromDays((double)result.AllowedDays);
                ws.Cell(row, 1).Value = "LAYTIME ALLOWED";
                ws.Cell(row, 5).Value = (int)allowedTs.TotalDays;
                ws.Cell(row, 6).Value = allowedTs.Hours;
                ws.Cell(row, 7).Value = allowedTs.Minutes;
                StyleAllowedRow(ws.Row(row));
                row++;
            }

            // ── Segments Rows ─────────────────────────────────────────────
            decimal balanceDays = result?.AllowedDays ?? 0;

            foreach (var seg in segments)
            {
                var counted = seg.CountedDuration;
                var countedDays = (decimal)counted.TotalDays;
                balanceDays -= countedDays;

                var balTs = TimeSpan.FromDays(Math.Abs((double)balanceDays));
                bool negative = balanceDays < 0;

                // Date + Day
                ws.Cell(row, 1).Value = seg.From.ToString("dd.MM.yyyy ddd").ToUpper();
                ws.Cell(row, 2).Value = seg.From.ToString("HH:mm");
                ws.Cell(row, 3).Value = seg.To.ToString("HH:mm");

                // Counted duration
                ws.Cell(row, 5).Value = (int)counted.TotalDays;
                ws.Cell(row, 6).Value = counted.Hours;
                ws.Cell(row, 7).Value = counted.Minutes;

                // Balance
                ws.Cell(row, 8).Value = negative ? -(int)balTs.TotalDays : (int)balTs.TotalDays;
                ws.Cell(row, 9).Value = negative ? -balTs.Hours : balTs.Hours;
                ws.Cell(row, 10).Value = negative ? -balTs.Minutes : balTs.Minutes;

                // Remarks
                ws.Cell(row, 11).Value = FormatSegmentRemark(seg);

                // Color coding
                var rowStyle = ws.Row(row).Style;
                rowStyle.Font.SetFontSize(10);

                if (seg.ImpactType == LaytimeImpactType.NoCount)
                    rowStyle.Fill.SetBackgroundColor(XLColor.LightYellow);
                else if (seg.ImpactType == LaytimeImpactType.ProRata)
                    rowStyle.Fill.SetBackgroundColor(XLColor.LightBlue);
                else if (negative)
                    rowStyle.Fill.SetBackgroundColor(XLColor.LightSalmon);

                StyleDataRow(ws.Row(row));
                row++;
            }

            // ── Totals ────────────────────────────────────────────────────
            if (result is not null)
            {
                row++;
                WriteTotalsBlock(ws, ref row, result);
            }

            // ── Legend ────────────────────────────────────────────────────
            row++;
            WriteLegend(ws, row);

            // ── Borders on table ─────────────────────────────────────────

            var firstRow = ws.FirstRowUsed();
            if (firstRow != null)
            {
                ws.Range(firstRow.RowNumber() + 9, 1, row - 3, 11)
                    .Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Hair);
            }
        }

        // ─────────────────────────────────────────────────────────────────

        private static void BuildSummarySheet(
            IXLWorksheet ws,
            LaytimeCalculation calc,
            CharterTerms charterTerms)
        {
            var result = calc.Result;
            ws.Column(1).Width = 35;
            ws.Column(2).Width = 25;

            int row = 1;

            WriteSheetTitle(ws, ref row, "LAYTIME SUMMARY");

            WriteKV(ws, ref row, "Voyage ID", calc.VoyageId.ToString());
            WriteKV(ws, ref row, "Contract ID", calc.ContractId.ToString());
            WriteKV(ws, ref row, "Operation Type", calc.OperationType.ToString());
            WriteKV(ws, ref row, "Status", calc.Status.ToString());
            WriteKV(ws, ref row, "Cargo Qty (MT)", $"{calc.CargoQuantityMt:N3}");
            WriteKV(ws, ref row, "Laytime Commenced",
                calc.LaytimeCommencedAt?.ToString("dd MMM yyyy HH:mm") ?? "—");
            WriteKV(ws, ref row, "Laytime Completed",
                calc.LaytimeCompletedAt?.ToString("dd MMM yyyy HH:mm") ?? "—");
            row++;

            WriteKV(ws, ref row, "Demurrage Rate (USD/Day)",
                $"{charterTerms.LaytimeTerms.Demurrage.RateUsdPerDay:N2}");

            if (charterTerms.LaytimeTerms.Despatch is not null)
                WriteKV(ws, ref row, "Despatch Rate (USD/Day)",
                    $"{charterTerms.LaytimeTerms.Despatch.RateUsdPerDay:N2}");
            row++;

            if (result is not null)
            {
                WriteKV(ws, ref row, "Laytime Allowed (Days)", $"{result.AllowedDays:F4}");
                WriteKV(ws, ref row, "Laytime Used (Days)", $"{result.UsedDays:F4}");
                WriteKV(ws, ref row, "Balance (Days)", $"{result.BalanceDays:F4}");
                row++;

                var finalCell = ws.Cell(row, 1);
                var valCell = ws.Cell(row, 2);

                if (result.IsDemurrage)
                {
                    finalCell.Value = "DEMURRAGE AMOUNT (USD)";
                    valCell.Value = result.DemurrageAmount;
                    valCell.Style.Font.SetFontColor(XLColor.Red).Font.SetBold(true);
                }
                else
                {
                    finalCell.Value = "DESPATCH AMOUNT (USD)";
                    valCell.Value = result.DespatchAmount;
                    valCell.Style.Font.SetFontColor(XLColor.DarkGreen).Font.SetBold(true);
                }

                finalCell.Style.Font.SetBold(true);
                valCell.Style.NumberFormat.Format = "#,##0.00";
            }
        }

        // ─────────────────────────────────────────────────────────────────

        private static void BuildSofEventsSheet(
            IXLWorksheet ws,
            LaytimeCalculation calc)
        {
            ws.Column(1).Width = 22;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 15;
            ws.Column(4).Width = 10;
            ws.Column(5).Width = 35;

            int row = 1;
            WriteSheetTitle(ws, ref row, "SOF EVENTS");

            // Header
            string[] headers = ["Event Time", "Event Type", "Impact", "Factor", "Description"];
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                StyleColumnHeader(ws.Cell(row, i + 1));
            }
            row++;

            foreach (var ev in calc.SofEvents.OrderBy(e => e.EventTime))
            {
                ws.Cell(row, 1).Value = ev.EventTime.ToString("dd MMM yyyy HH:mm");
                ws.Cell(row, 2).Value = FormatEnumLabel(ev.EventType.ToString());
                ws.Cell(row, 3).Value = ev.ImpactType.ToString();
                ws.Cell(row, 4).Value = ev.Factor;
                ws.Cell(row, 5).Value = ev.Description ?? string.Empty;

                if (ev.ImpactType == LaytimeImpactType.NoCount)
                    ws.Row(row).Style.Fill.SetBackgroundColor(XLColor.LightYellow);

                ws.Row(row).Style.Font.SetFontSize(10);
                row++;
            }

            ws.RangeUsed()?.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                                        .Border.SetInsideBorder(XLBorderStyleValues.Hair);
        }

        // ─────────────────────────────────────────────────────────────────

        private static void BuildExceptionsSheet(
            IXLWorksheet ws,
            LaytimeCalculation calc)
        {
            ws.Column(1).Width = 22;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 12;
            ws.Column(5).Width = 12;
            ws.Column(6).Width = 35;

            int row = 1;
            WriteSheetTitle(ws, ref row, "EXCEPTIONS / INTERRUPTIONS");

            string[] headers = ["From", "To", "Exception Type", "Factor", "Duration", "Notes"];
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                StyleColumnHeader(ws.Cell(row, i + 1));
            }
            row++;

            foreach (var ex in calc.Exceptions.OrderBy(e => e.From))
            {
                ws.Cell(row, 1).Value = ex.From.ToString("dd MMM yyyy HH:mm");
                ws.Cell(row, 2).Value = ex.To.ToString("dd MMM yyyy HH:mm");
                ws.Cell(row, 3).Value = FormatEnumLabel(ex.ExceptionType.ToString());
                ws.Cell(row, 4).Value = ex.Factor;
                ws.Cell(row, 5).Value = FormatDuration(ex.Duration);
                ws.Cell(row, 6).Value = ex.Notes ?? string.Empty;

                ws.Row(row).Style
                  .Font.SetFontSize(10)
                  .Fill.SetBackgroundColor(XLColor.LightYellow);
                row++;
            }

            if (calc.Exceptions.Count == 0)
            {
                ws.Cell(row, 1).Value = "No exceptions recorded.";
                ws.Cell(row, 1).Style.Font.SetItalic(true);
            }

            ws.RangeUsed()?.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                                        .Border.SetInsideBorder(XLBorderStyleValues.Hair);
        }

        // ─────────────────────────────────────────────────────────────────

        private static void BuildDelaySheet(
            IXLWorksheet ws,
            LaytimeCalculation calc,
            CharterTerms charterTerms)
        {
            ws.Column(1).Width = 22;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 14;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 14;
            ws.Column(7).Width = 35;

            int row = 1;
            WriteSheetTitle(ws, ref row, "DELAY REPORT");

            string[] headers =
                ["From", "To", "Type", "Duration", "Counted", "Factor", "Notes / Reason"];
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                StyleColumnHeader(ws.Cell(row, i + 1));
            }
            row++;

            // الـ Segments التي تكون NoCount أو ProRata هي فترات التأخير
            var delays = calc.Segments
                .Where(s => s.ImpactType != LaytimeImpactType.FullCount)
                .OrderBy(s => s.From)
                .ToList();

            TimeSpan totalDelay = TimeSpan.Zero;
            TimeSpan totalCounted = TimeSpan.Zero;

            foreach (var seg in delays)
            {
                ws.Cell(row, 1).Value = seg.From.ToString("dd MMM yyyy HH:mm");
                ws.Cell(row, 2).Value = seg.To.ToString("dd MMM yyyy HH:mm");
                ws.Cell(row, 3).Value = seg.ImpactType.ToString();
                ws.Cell(row, 4).Value = FormatDuration(seg.Duration);
                ws.Cell(row, 5).Value = FormatDuration(seg.CountedDuration);
                ws.Cell(row, 6).Value = seg.Factor;
                ws.Cell(row, 7).Value = seg.Reason ?? string.Empty;

                ws.Row(row).Style
                  .Font.SetFontSize(10)
                  .Fill.SetBackgroundColor(
                      seg.ImpactType == LaytimeImpactType.NoCount
                          ? XLColor.LightYellow
                          : XLColor.LightBlue);

                totalDelay += seg.Duration;
                totalCounted += seg.CountedDuration;
                row++;
            }

            // Totals
            row++;
            ws.Cell(row, 3).Value = "TOTAL";
            ws.Cell(row, 4).Value = FormatDuration(totalDelay);
            ws.Cell(row, 5).Value = FormatDuration(totalCounted);
            ws.Row(row).Style.Font.SetBold(true);

            ws.RangeUsed()?.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                                        .Border.SetInsideBorder(XLBorderStyleValues.Hair);
        }

        // ─────────────────────────────────────────────────────────────────

        private static void BuildContractOverviewSheet(
            IXLWorksheet ws,
            IReadOnlyList<LaytimeCalculation> calculations,
            CharterTerms charterTerms)
        {
            ws.Column(1).Width = 8;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 18;
            ws.Column(4).Width = 14;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 14;
            ws.Column(7).Width = 16;
            ws.Column(8).Width = 16;
            ws.Column(9).Width = 18;

            int row = 1;
            WriteSheetTitle(ws, ref row, $"CONTRACT LAYTIME OVERVIEW — Contract #{charterTerms.ContractId}");

            string[] headers =
            [
                "#", "Operation", "Status",
                "Allowed Days", "Used Days", "Balance Days",
                "Demurrage (USD)", "Despatch (USD)", "Result"
            ];
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                StyleColumnHeader(ws.Cell(row, i + 1));
            }
            row++;

            decimal totalDemurrage = 0;
            decimal totalDespatch = 0;
            int seq = 1;

            foreach (var calc in calculations)
            {
                ws.Cell(row, 1).Value = seq++;
                ws.Cell(row, 2).Value = calc.OperationType.ToString();
                ws.Cell(row, 3).Value = calc.Status.ToString();
                ws.Cell(row, 4).Value = calc.Result?.AllowedDays ?? 0;
                ws.Cell(row, 5).Value = calc.Result?.UsedDays ?? 0;
                ws.Cell(row, 6).Value = calc.Result?.BalanceDays ?? 0;

                if (calc.Result is not null)
                {
                    ws.Cell(row, 7).Value = calc.Result.DemurrageAmount;
                    ws.Cell(row, 8).Value = calc.Result.DespatchAmount;
                    ws.Cell(row, 9).Value = calc.Result.IsDemurrage ? "DEMURRAGE" : "DESPATCH";

                    ws.Cell(row, 9).Style.Font
                      .SetFontColor(calc.Result.IsDemurrage ? XLColor.Red : XLColor.DarkGreen);
                    ws.Style.Font.SetBold(true);

                    totalDemurrage += calc.Result.DemurrageAmount;
                    totalDespatch += calc.Result.DespatchAmount;
                }

                ws.Row(row).Style.Font.SetFontSize(10);
                row++;
            }

            // Net total row
            row++;
            ws.Cell(row, 6).Value = "NET TOTAL";
            ws.Cell(row, 7).Value = totalDemurrage;
            ws.Cell(row, 8).Value = totalDespatch;

            decimal net = totalDemurrage - totalDespatch;
            ws.Cell(row, 9).Value = net >= 0
                ? $"DEM: {net:N2} USD"
                : $"DES: {Math.Abs(net):N2} USD";

            ws.Cell(row, 9).Style.Font
              .SetFontColor(net >= 0 ? XLColor.Red : XLColor.DarkGreen)
              .Font.SetBold(true);

            ws.Row(row).Style.Font.SetBold(true)
                              .Fill.SetBackgroundColor(XLColor.LightGray);

            ws.RangeUsed()?.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                                        .Border.SetInsideBorder(XLBorderStyleValues.Hair);
        }

        // ═══════════════════════════════════════════════════════════════════
        // PDF — Stub (iText7 / QuestPDF في Infrastructure مستقبلاً)
        // ═══════════════════════════════════════════════════════════════════

        private static byte[] GenerateSummaryPdf(
            LaytimeCalculation calc,
            CharterTerms charterTerms)
            => throw new NotImplementedException(
                "PDF generation requires iText7 or QuestPDF — register IPdfRenderer in Infrastructure.");

        private static byte[] GenerateDetailedPdf(
            LaytimeCalculation calc,
            CharterTerms charterTerms)
            => throw new NotImplementedException(
                "PDF generation requires iText7 or QuestPDF — register IPdfRenderer in Infrastructure.");

        private static byte[] GenerateDelayPdf(
            LaytimeCalculation calc,
            CharterTerms charterTerms)
            => throw new NotImplementedException(
                "PDF generation requires iText7 or QuestPDF — register IPdfRenderer in Infrastructure.");

        private static byte[] GenerateContractPdf(
            IReadOnlyList<LaytimeCalculation> calculations,
            CharterTerms charterTerms)
            => throw new NotImplementedException(
                "PDF generation requires iText7 or QuestPDF — register IPdfRenderer in Infrastructure.");

        // ═══════════════════════════════════════════════════════════════════
        // Style Helpers
        // ═══════════════════════════════════════════════════════════════════

        private static void StyleHeader(IXLCell cell, string value)
        {
            cell.Value = value;
            cell.Style
                .Font.SetBold(true)
                .Font.SetFontSize(14)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.DarkBlue)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        private static void StyleColumnHeader(IXLCell cell)
        {
            cell.Style
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.DarkSlateBlue)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        private static void StyleDataRow(IXLRow row)
        {
            row.Style
               .Font.SetFontSize(10)
               .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        }

        private static void StyleAllowedRow(IXLRow row)
        {
            row.Style
               .Font.SetBold(true)
               .Fill.SetBackgroundColor(XLColor.LightGreen);
        }

        private static int WriteInfoRow(IXLWorksheet ws, int row, string label, string value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.SetBold(true);
            ws.Cell(row, 2).Value = value;
            ws.Range(row, 2, row, 11).Merge();
            return row + 1;
        }

        private static void WriteSheetTitle(IXLWorksheet ws, ref int row, string title)
        {
            ws.Cell(row, 1).Value = title;
            ws.Row(row).Style
              .Font.SetBold(true)
              .Font.SetFontSize(13)
              .Font.SetFontColor(XLColor.White)
              .Fill.SetBackgroundColor(XLColor.DarkBlue);
            row += 2;
        }

        private static void WriteKV(IXLWorksheet ws, ref int row, string key, string value)
        {
            ws.Cell(row, 1).Value = key;
            ws.Cell(row, 1).Style.Font.SetBold(true);
            ws.Cell(row, 2).Value = value;
            row++;
        }

        private static void WriteTableHeader(IXLWorksheet ws, int row)
        {
            string[] cols = ["DATE", "FROM", "TO", "D", "H", "M", "D", "H", "M", "", "REMARKS"];
            for (int i = 0; i < cols.Length; i++)
            {
                ws.Cell(row, i + 1).Value = cols[i];
                StyleColumnHeader(ws.Cell(row, i + 1));
            }

            // Sub-headers
            ws.Cell(row - 1, 4).Value = "TIME USED";
            ws.Range(row - 1, 4, row - 1, 6).Merge()
              .Style.Font.SetBold(true)
              .Fill.SetBackgroundColor(XLColor.SteelBlue)
              .Font.SetFontColor(XLColor.White)
              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(row - 1, 7).Value = "BALANCE";
            ws.Range(row - 1, 7, row - 1, 9).Merge()
              .Style.Font.SetBold(true)
              .Fill.SetBackgroundColor(XLColor.SteelBlue)
              .Font.SetFontColor(XLColor.White)
              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        private static int WriteAllowedUsed(
     IXLWorksheet ws,
     int row,
     Domain.ValueObjects.Laytime.LaytimeResult result)
        {
            var allowedTs = TimeSpan.FromDays((double)result.AllowedDays);
            var usedTs = TimeSpan.FromDays((double)result.UsedDays);
            var lostTs = TimeSpan.FromDays(Math.Abs((double)result.BalanceDays));

            row = WriteTs(ws, row,
                "LAYTIME ALLOWED",
                allowedTs,
                XLColor.LightGreen);

            row = WriteTs(ws, row,
                "ACTUAL TIME USED",
                usedTs,
                XLColor.LightBlue);

            row = WriteTs(ws, row,
                result.IsDemurrage ? "TIME LOST" : "TIME SAVED",
                lostTs,
                result.IsDemurrage
                    ? XLColor.LightSalmon
                    : XLColor.LightGreen);

            // DEM / DESP Amount
            ws.Cell(row, 1).Value = result.IsDemurrage
                ? $"DEMURRAGE: USD {result.DemurrageAmount:N2}"
                : $"DESPATCH:  USD {result.DespatchAmount:N2}";

            ws.Range(row, 1, row, 10).Merge();

            var style = ws.Cell(row, 1).Style;
            style.Font.SetBold(true);
            style.Font.FontSize = 12;
            style.Font.FontColor = result.IsDemurrage ? XLColor.Red : XLColor.DarkGreen;

            ws.Cell(row, 1).Style.Alignment
                .SetHorizontal(XLAlignmentHorizontalValues.Center);

            row++;

            return row;
        }

        private static int WriteTs(
    IXLWorksheet ws,
    int row,
    string label,
    TimeSpan ts,
    XLColor color)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.SetBold(true);

            ws.Cell(row, 5).Value = (int)ts.TotalDays;
            ws.Cell(row, 6).Value = ts.Hours;
            ws.Cell(row, 7).Value = ts.Minutes;

            ws.Range(row, 1, row, 10)
                .Style.Fill.SetBackgroundColor(color);

            return row + 1;
        }


        private static void WriteTotalsBlock(
            IXLWorksheet ws,
            ref int row,
            Domain.ValueObjects.Laytime.LaytimeResult result)
        {
            var usedTs = TimeSpan.FromDays((double)result.UsedDays);
            var allowedTs = TimeSpan.FromDays((double)result.AllowedDays);

            ws.Cell(row, 1).Value = "TOTAL";
            ws.Cell(row, 5).Value = (int)usedTs.TotalDays;
            ws.Cell(row, 6).Value = usedTs.Hours;
            ws.Cell(row, 7).Value = usedTs.Minutes;
            ws.Row(row).Style.Font.SetBold(true);
            row++;

            ws.Cell(row, 1).Value = "LAYTIME ALLOWED";
            ws.Cell(row, 5).Value = (int)allowedTs.TotalDays;
            ws.Cell(row, 6).Value = allowedTs.Hours;
            ws.Cell(row, 7).Value = allowedTs.Minutes;
            ws.Row(row).Style.Font.SetBold(true).Fill.SetBackgroundColor(XLColor.LightGreen);
            row++;

            ws.Cell(row, 1).Value = "ACTUAL TIME USED";
            ws.Cell(row, 5).Value = (int)usedTs.TotalDays;
            ws.Cell(row, 6).Value = usedTs.Hours;
            ws.Cell(row, 7).Value = usedTs.Minutes;
            ws.Row(row).Style.Font.SetBold(true).Fill.SetBackgroundColor(XLColor.LightBlue);
            row++;

            var lostTs = TimeSpan.FromDays(Math.Abs((double)result.BalanceDays));
            ws.Cell(row, 1).Value = result.IsDemurrage ? "LOST TIME" : "TIME SAVED";
            ws.Cell(row, 5).Value = result.IsDemurrage ? -(int)lostTs.TotalDays : (int)lostTs.TotalDays;
            ws.Cell(row, 6).Value = result.IsDemurrage ? -lostTs.Hours : lostTs.Hours;
            ws.Cell(row, 7).Value = result.IsDemurrage ? -lostTs.Minutes : lostTs.Minutes;
            ws.Row(row).Style.Font.SetBold(true);
            ws.Row(row).Style.Font.FontColor = result.IsDemurrage ? XLColor.Red : XLColor.DarkGreen;
            row++;
        }

        private static void WriteLegend(IXLWorksheet ws, int row)
        {
            ws.Cell(row, 1).Value = "LEGEND:";
            ws.Cell(row, 1).Style.Font.SetBold(true);

            ws.Cell(row, 2).Value = "Full Count";
            ws.Cell(row, 2).Style.Fill.SetBackgroundColor(XLColor.White);

            ws.Cell(row, 3).Value = "Not Counted";
            ws.Cell(row, 3).Style.Fill.SetBackgroundColor(XLColor.LightYellow);

            ws.Cell(row, 4).Value = "Pro Rata";
            ws.Cell(row, 4).Style.Fill.SetBackgroundColor(XLColor.LightBlue);

            ws.Cell(row, 5).Value = "Demurrage";
            ws.Cell(row, 5).Style.Fill.SetBackgroundColor(XLColor.LightSalmon);
        }

        // ═══════════════════════════════════════════════════════════════════
        // General Helpers
        // ═══════════════════════════════════════════════════════════════════

        private static byte[] ToBytes(XLWorkbook workbook)
        {
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        private static string FormatDuration(TimeSpan ts)
        {
            int days = (int)ts.TotalDays;
            return days > 0
                ? $"{days}d {ts.Hours:D2}h {ts.Minutes:D2}m"
                : $"{ts.Hours:D2}h {ts.Minutes:D2}m";
        }

        private static string FormatSegmentRemark(LaytimeSegment seg)
        {
            var impact = seg.ImpactType switch
            {
                LaytimeImpactType.FullCount => "IN ACC",
                LaytimeImpactType.NoCount => "NOT ACC",
                LaytimeImpactType.ProRata => $"PRO RATA {seg.Factor:P0}",
                _ => string.Empty
            };
            return string.IsNullOrEmpty(seg.Reason)
                ? impact
                : $"{FormatEnumLabel(seg.Reason)} / {impact}";
        }

        private static string FormatEnumLabel(string enumValue) =>
            string.Concat(enumValue.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));

        private static CargoOperationTerms? GetOperationTerms(
            CharterTerms charterTerms,
            OperationType operationType) =>
            operationType == OperationType.Loading
                ? charterTerms.LaytimeTerms.Loading
                : charterTerms.LaytimeTerms.Discharging;
    }
}
