using System.Globalization;
using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Rules;

namespace Marilog.Shared.UI.Pages.DocumentTemplates
{
    // Labels, sample values and defaults for the template designer. No services, no state.
    // Sizes and positions are in PDF points, like the saved template.
    public static class DocumentTemplateUi
    {
        // Same values as DocumentTemplateRenderer, so the designer draws boxes the way the PDF does.
        public const float BoxPadding = 2;
        public const float BorderWidth = 0.75f;

        public static readonly int[] Zooms = [75, 100, 125, 150];

        // Document fields offered in the palette, in display order.
        public static readonly TemplateElementKey[] Fields =
        [
            TemplateElementKey.DocNumber,
            TemplateElementKey.DocType,
            TemplateElementKey.DocDate,
            TemplateElementKey.DocDue,
            TemplateElementKey.Reference,
            TemplateElementKey.Supplier,
            TemplateElementKey.Buyer,
            TemplateElementKey.Vessel,
            TemplateElementKey.Port,
            TemplateElementKey.Voyage,
            TemplateElementKey.Currency,
            TemplateElementKey.TotalAmount,
            TemplateElementKey.AdjustmentsTotal,
            TemplateElementKey.NetAmount
        ];

        // CSS pixels per PDF point at a zoom level (100% = real size on a 96 dpi screen).
        public static float PxPerPt(int zoom) => zoom / 100f * 96f / 72f;

        public static string Css(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        public static string Label(TemplateElementKey key) => key switch
        {
            TemplateElementKey.StaticText => "Fixed text",
            TemplateElementKey.Image => "Image",
            TemplateElementKey.ItemsTable => "Items table",
            TemplateElementKey.DocNumber => "Document number",
            TemplateElementKey.DocType => "Document type",
            TemplateElementKey.DocDate => "Date",
            TemplateElementKey.DocDue => "Due date",
            TemplateElementKey.Reference => "Reference",
            TemplateElementKey.Supplier => "Supplier",
            TemplateElementKey.Buyer => "Buyer",
            TemplateElementKey.Vessel => "Vessel",
            TemplateElementKey.Port => "Port",
            TemplateElementKey.Voyage => "Voyage",
            TemplateElementKey.Currency => "Currency",
            TemplateElementKey.TotalAmount => "Total amount",
            TemplateElementKey.AdjustmentsTotal => "Adjustments",
            TemplateElementKey.NetAmount => "Net amount",
            _ => key.ToString()
        };

        public static string ScopeLabel(TemplatePageScope scope) => scope switch
        {
            TemplatePageScope.First => "First page only",
            TemplatePageScope.Last => "Last page only",
            _ => "Every page"
        };

        // The same sample values the server uses for a preview without a document.
        public static string SampleValue(TemplateElementKey key)
        {
            var today = DateTime.Today;
            return key switch
            {
                TemplateElementKey.DocNumber => "INV-0001",
                TemplateElementKey.DocType => "Invoice",
                TemplateElementKey.DocDate => today.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                TemplateElementKey.DocDue => today.AddDays(30).ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                TemplateElementKey.Reference => "REF-0001",
                TemplateElementKey.Supplier => "Supplier Company",
                TemplateElementKey.Buyer => "Buyer Company",
                TemplateElementKey.Vessel => "MV SAMPLE",
                TemplateElementKey.Port => "Port Name",
                TemplateElementKey.Voyage => "V-001",
                TemplateElementKey.Currency => "USD",
                TemplateElementKey.TotalAmount => "12,500.00",
                TemplateElementKey.AdjustmentsTotal => "-500.00",
                TemplateElementKey.NetAmount => "12,000.00",
                _ => string.Empty
            };
        }

        // Sample item lines for the items table preview: ProductName, Quantity, Unit, UnitPrice, LineTotal.
        public static readonly string[][] SampleItems =
        [
            ["Freight", "1", "LS", "10,000.00", "10,000.00"],
            ["Demurrage", "2.5", "Days", "800.00", "2,000.00"],
            ["Port expenses", "1", "LS", "500.00", "500.00"]
        ];

        public static string SampleCell(TemplateColumnKey key, string[] row) => key switch
        {
            TemplateColumnKey.ProductName => row[0],
            TemplateColumnKey.Quantity => row[1],
            TemplateColumnKey.Unit => row[2],
            TemplateColumnKey.UnitPrice => row[3],
            TemplateColumnKey.LineTotal => row[4],
            _ => string.Empty
        };

        public static bool IsNumberColumn(TemplateColumnKey key)
            => key is TemplateColumnKey.Quantity or TemplateColumnKey.UnitPrice or TemplateColumnKey.LineTotal;

        // Totals belong on the last page, the document number and date on every page, the rest on the first.
        public static TemplatePageScope DefaultScope(TemplateElementKey key) => key switch
        {
            TemplateElementKey.TotalAmount or TemplateElementKey.AdjustmentsTotal or TemplateElementKey.NetAmount
                => TemplatePageScope.Last,
            TemplateElementKey.DocNumber or TemplateElementKey.DocDate
                or TemplateElementKey.StaticText or TemplateElementKey.Image
                => TemplatePageScope.All,
            _ => TemplatePageScope.First
        };

        public static List<DocumentTemplateColumnDto> DefaultColumns() =>
        [
            new() { Key = TemplateColumnKey.ProductName, Width = 4, Title = DocumentTemplateRules.DefaultColumnTitle(TemplateColumnKey.ProductName) },
            new() { Key = TemplateColumnKey.Quantity, Width = 1, Title = DocumentTemplateRules.DefaultColumnTitle(TemplateColumnKey.Quantity) },
            new() { Key = TemplateColumnKey.Unit, Width = 1, Title = DocumentTemplateRules.DefaultColumnTitle(TemplateColumnKey.Unit) },
            new() { Key = TemplateColumnKey.UnitPrice, Width = 1.5f, Title = DocumentTemplateRules.DefaultColumnTitle(TemplateColumnKey.UnitPrice) },
            new() { Key = TemplateColumnKey.LineTotal, Width = 1.5f, Title = DocumentTemplateRules.DefaultColumnTitle(TemplateColumnKey.LineTotal) }
        ];

        // A new element with a sensible size, placed at (x, y) and kept inside the page.
        public static DocumentTemplateElementDto NewElement(TemplateElementKey key, float x, float y, float pageWidth, float pageHeight)
        {
            var (width, height) = key switch
            {
                TemplateElementKey.StaticText => (200f, 40f),
                TemplateElementKey.Image => (150f, 80f),
                TemplateElementKey.ItemsTable => (pageWidth - 80f, 300f),
                _ => (150f, 18f)
            };

            var element = new DocumentTemplateElementDto
            {
                Key = key,
                PageScope = DefaultScope(key),
                Width = Math.Min(width, pageWidth),
                Height = Math.Min(height, pageHeight),
                FontSize = key == TemplateElementKey.ItemsTable ? 9 : 10,
                Text = key == TemplateElementKey.StaticText ? "Text" : null,
                Columns = key == TemplateElementKey.ItemsTable ? DefaultColumns() : []
            };

            element.X = Clamp(x, 0, pageWidth - element.Width);
            element.Y = Clamp(y, 0, pageHeight - element.Height);
            return element;
        }

        // Shrinks and moves an element so it stays inside the page (after a page size or orientation change).
        public static void KeepInside(DocumentTemplateElementDto element, float pageWidth, float pageHeight)
        {
            element.Width = Math.Min(element.Width, pageWidth);
            element.Height = Math.Min(element.Height, pageHeight);
            element.X = Clamp(element.X, 0, pageWidth - element.Width);
            element.Y = Clamp(element.Y, 0, pageHeight - element.Height);
        }

        public static float Clamp(float value, float min, float max) => Math.Min(Math.Max(value, min), Math.Max(min, max));
    }
}