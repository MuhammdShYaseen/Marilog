using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using System.Net;
using System.Text;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Marilog.Infrastructure.Services.OCR
{
    /// <summary>
    /// Reads the text of Word, Excel, PowerPoint and plain-text files without any external program,
    /// and builds a simple HTML page of the same content for the preview panel.
    /// Every value is HTML-encoded.
    /// </summary>
    public static class OfficeContentExtractor
    {
        private const int MaxRowsPerSheet = 5000;

        public static bool CanRead(string path) =>
            Path.GetExtension(path).ToLowerInvariant() is ".docx" or ".xlsx" or ".pptx" or ".txt";

        public static (string Text, string Html) Read(string path) =>
            Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".docx" => ReadWord(path),
                ".xlsx" => ReadExcel(path),
                ".pptx" => ReadPowerPoint(path),
                _ => ReadPlainText(path)
            };

        private static (string, string) ReadWord(string path)
        {
            using var doc = WordprocessingDocument.Open(path, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            var text = new StringBuilder();
            var html = new StringBuilder();

            if (body is not null)
            {
                foreach (var element in body.ChildElements)
                {
                    if (element is W.Paragraph paragraph)
                    {
                        text.AppendLine(paragraph.InnerText);
                        html.Append("<p dir=\"auto\">").Append(Enc(paragraph.InnerText)).Append("</p>");
                    }
                    else if (element is W.Table table)
                    {
                        AppendTable(text, html, table.Elements<W.TableRow>()
                            .Select(r => r.Elements<W.TableCell>().Select(c => c.InnerText).ToList()));
                    }
                }
            }

            return (text.ToString(), Page(html.ToString()));
        }

        private static (string, string) ReadExcel(string path)
        {
            using var workbook = new XLWorkbook(path);
            var text = new StringBuilder();
            var html = new StringBuilder();

            foreach (var sheet in workbook.Worksheets)
            {
                var used = sheet.RangeUsed();
                if (used is null) continue;

                text.AppendLine(sheet.Name);
                html.Append("<h3>").Append(Enc(sheet.Name)).Append("</h3>");

                AppendTable(text, html, used.Rows().Take(MaxRowsPerSheet)
                    .Select(row => Enumerable.Range(1, row.CellCount())
                        .Select(i => row.Cell(i).GetFormattedString())
                        .ToList()));
            }

            return (text.ToString(), Page(html.ToString()));
        }

        private static (string, string) ReadPowerPoint(string path)
        {
            using var doc = PresentationDocument.Open(path, false);
            var part = doc.PresentationPart;
            var slideIds = part?.Presentation?.SlideIdList?.Elements<P.SlideId>() ?? Enumerable.Empty<P.SlideId>();
            var text = new StringBuilder();
            var html = new StringBuilder();
            var number = 0;

            foreach (var slideId in slideIds)
            {
                number++;
                if (slideId.RelationshipId?.Value is not { } relId || part!.GetPartById(relId) is not SlidePart slidePart)
                    continue;

                html.Append("<h3>Slide ").Append(number).Append("</h3>");

                var paragraphs = slidePart.Slide?.Descendants<A.Paragraph>() ?? Enumerable.Empty<A.Paragraph>();
                foreach (var paragraph in paragraphs)
                {
                    var line = string.Concat(paragraph.Descendants<A.Text>().Select(t => t.Text));
                    if (line.Length == 0) continue;

                    text.AppendLine(line);
                    html.Append("<p dir=\"auto\">").Append(Enc(line)).Append("</p>");
                }
            }

            return (text.ToString(), Page(html.ToString()));
        }

        private static (string, string) ReadPlainText(string path)
        {
            var content = File.ReadAllText(path);
            return (content, Page("<p dir=\"auto\">" + Enc(content) + "</p>"));
        }

        private static void AppendTable(StringBuilder text, StringBuilder html, IEnumerable<List<string>> rows)
        {
            html.Append("<table>");
            foreach (var cells in rows)
            {
                text.AppendLine(string.Join('\t', cells));
                html.Append("<tr>");
                foreach (var cell in cells)
                    html.Append("<td dir=\"auto\">").Append(Enc(cell)).Append("</td>");
                html.Append("</tr>");
            }
            html.Append("</table>");
        }

        private static string Enc(string value) => WebUtility.HtmlEncode(value);

        private static string Page(string body) =>
            "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>" +
            "body{font-family:'Segoe UI',Arial,sans-serif;font-size:13px;margin:16px}" +
            "p{margin:4px 0;white-space:pre-wrap}h3{font-size:14px;margin:16px 0 4px}" +
            "table{border-collapse:collapse;margin:8px 0}" +
            "td{border:1px solid #ccc;padding:2px 6px;vertical-align:top;white-space:pre-wrap}" +
            "</style></head><body>" + body + "</body></html>";
    }
}