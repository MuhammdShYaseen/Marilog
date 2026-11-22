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

        public const string DefaultTextColor = "#000000";
        public const string DefaultFill = "#E0E0E0";   // first colour offered when a background is switched on
        public static readonly int[] Zooms = [75, 100, 125, 150];

        // Fields offered in the palette, by group, in display order.
        public static readonly (string Title, TemplateElementKey[] Keys)[] FieldGroups =
        [
            ("Document fields",
            [
                TemplateElementKey.DocNumber,
                TemplateElementKey.DocType,
                TemplateElementKey.DocDate,
                TemplateElementKey.DocDue,
                TemplateElementKey.Reference,
                TemplateElementKey.Vessel,
                TemplateElementKey.Port,
                TemplateElementKey.Voyage,
                TemplateElementKey.Currency
            ]),
            ("Supplier",
            [
                TemplateElementKey.Supplier,
                TemplateElementKey.SupplierAddress,
                TemplateElementKey.SupplierCountry,
                TemplateElementKey.SupplierRegistrationNumber,
                TemplateElementKey.SupplierContactName
            ]),
            ("Buyer",
            [
                TemplateElementKey.Buyer,
                TemplateElementKey.BuyerAddress,
                TemplateElementKey.BuyerCountry,
                TemplateElementKey.BuyerRegistrationNumber,
                TemplateElementKey.BuyerContactName
            ]),
            ("Amounts",
            [
                TemplateElementKey.TotalAmount,
                TemplateElementKey.AdjustmentsTotal,
                TemplateElementKey.NetAmount,
                TemplateElementKey.TotalPaid,
                TemplateElementKey.RemainingBalance,
                TemplateElementKey.AmountInWords
            ]),
            ("Page",
            [
                TemplateElementKey.PageNumber
            ])
        ];

        // CSS pixels per PDF point at a zoom level (100% = real size on a 96 dpi screen).
        public static float PxPerPt(int zoom) => zoom / 100f * 96f / 72f;

        public static string Css(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        public static string Label(TemplateElementKey key) => key switch
        {
            TemplateElementKey.StaticText => "Fixed text",
            TemplateElementKey.Image => "Image",
            TemplateElementKey.ItemsTable => "Items table",
            TemplateElementKey.PaymentsTable => "Payments table",
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
            TemplateElementKey.TotalPaid => "Total paid",
            TemplateElementKey.RemainingBalance => "Balance due",
            TemplateElementKey.SupplierAddress => "Supplier address",
            TemplateElementKey.SupplierCountry => "Supplier country",
            TemplateElementKey.SupplierRegistrationNumber => "Supplier registration no.",
            TemplateElementKey.SupplierContactName => "Supplier contact",
            TemplateElementKey.BuyerAddress => "Buyer address",
            TemplateElementKey.BuyerCountry => "Buyer country",
            TemplateElementKey.BuyerRegistrationNumber => "Buyer registration no.",
            TemplateElementKey.BuyerContactName => "Buyer contact",
            TemplateElementKey.AmountInWords => "Net amount in words",
            TemplateElementKey.PageNumber => "Page number",
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
                TemplateElementKey.TotalPaid => "7,000.00",
                TemplateElementKey.RemainingBalance => "5,000.00",
                TemplateElementKey.SupplierAddress => "Street 1, Business Bay, Dubai",
                TemplateElementKey.SupplierCountry => "United Arab Emirates",
                TemplateElementKey.SupplierRegistrationNumber => "REG-0001",
                TemplateElementKey.SupplierContactName => "Supplier Contact",
                TemplateElementKey.BuyerAddress => "Street 2, Port Area, Monrovia",
                TemplateElementKey.BuyerCountry => "Liberia",
                TemplateElementKey.BuyerRegistrationNumber => "REG-0002",
                TemplateElementKey.BuyerContactName => "Buyer Contact",
                TemplateElementKey.AmountInWords => "Twelve Thousand US Dollars",
                TemplateElementKey.PageNumber => "Page 1 of 1",
                _ => string.Empty
            };
        }

        // Sample lines for the table previews, one value per column in DocumentTemplateRules.ColumnsFor order.
        // Items: ProductName, Quantity, Unit, UnitPrice, LineTotal.
        private static readonly string[][] SampleItems =
        [
            ["Freight", "1", "LS", "10,000.00", "10,000.00"],
            ["Demurrage", "2.5", "Days", "800.00", "2,000.00"],
            ["Port expenses", "1", "LS", "500.00", "500.00"]
        ];

        // Payments: PaymentDate, PaymentMethod, PaymentReference, PaymentNote, PaidAmount.
        private static readonly string[][] SamplePayments =
        [
            [DateTime.Today.AddDays(-10).ToString("dd MMM yyyy", CultureInfo.InvariantCulture), "SWIFT", "SWIFT-0001", "Advance", "5,000.00"],
            [DateTime.Today.AddDays(-2).ToString("dd MMM yyyy", CultureInfo.InvariantCulture), "Cash on board", "", "Cash to Master", "2,000.00"]
        ];

        public static string[][] SampleRows(TemplateElementKey table)
            => table == TemplateElementKey.PaymentsTable ? SamplePayments : SampleItems;

        public static string SampleCell(TemplateElementKey table, TemplateColumnKey key, string[] row)
        {
            var index = Array.IndexOf(DocumentTemplateRules.ColumnsFor(table), key);
            return index >= 0 && index < row.Length ? row[index] : string.Empty;
        }

        public static bool IsNumberColumn(TemplateColumnKey key) => DocumentTemplateRules.IsNumberColumn(key);

        // Totals belong on the last page, the document number, date and page number on every page, the rest on the first.
        public static TemplatePageScope DefaultScope(TemplateElementKey key) => key switch
        {
            TemplateElementKey.TotalAmount or TemplateElementKey.AdjustmentsTotal or TemplateElementKey.NetAmount
                or TemplateElementKey.TotalPaid or TemplateElementKey.RemainingBalance or TemplateElementKey.PaymentsTable
                or TemplateElementKey.AmountInWords
                => TemplatePageScope.Last,
            TemplateElementKey.DocNumber or TemplateElementKey.DocDate
                or TemplateElementKey.StaticText or TemplateElementKey.Image or TemplateElementKey.PageNumber
                => TemplatePageScope.All,
            _ => TemplatePageScope.First
        };

        // All columns of a table with their starting widths.
        public static List<DocumentTemplateColumnDto> DefaultColumns(TemplateElementKey table)
            => DocumentTemplateRules.ColumnsFor(table)
                .Select(key => new DocumentTemplateColumnDto
                {
                    Key = key,
                    Width = DefaultColumnWidth(key),
                    Title = DocumentTemplateRules.DefaultColumnTitle(key)
                })
                .ToList();

        private static float DefaultColumnWidth(TemplateColumnKey key) => key switch
        {
            TemplateColumnKey.ProductName => 4,
            TemplateColumnKey.Quantity or TemplateColumnKey.Unit => 1,
            TemplateColumnKey.PaymentReference => 2,
            TemplateColumnKey.PaymentNote => 2.5f,
            _ => 1.5f
        };

        // A new element with a sensible size, placed at (x, y) and kept inside the page.
        public static DocumentTemplateElementDto NewElement(TemplateElementKey key, float x, float y, float pageWidth, float pageHeight)
        {
            var (width, height) = key switch
            {
                TemplateElementKey.StaticText => (200f, 40f),
                TemplateElementKey.Image => (150f, 80f),
                TemplateElementKey.ItemsTable => (pageWidth - 80f, 300f),
                TemplateElementKey.PaymentsTable => (pageWidth - 80f, 90f),
                TemplateElementKey.SupplierAddress or TemplateElementKey.BuyerAddress => (200f, 40f),
                TemplateElementKey.AmountInWords => (350f, 18f),
                TemplateElementKey.PageNumber => (100f, 18f),
                _ => (150f, 18f)
            };

            var element = new DocumentTemplateElementDto
            {
                Key = key,
                PageScope = DefaultScope(key),
                Width = Math.Min(width, pageWidth),
                Height = Math.Min(height, pageHeight),
                FontSize = DocumentTemplateRules.IsTable(key) ? 9 : 10,
                Text = key == TemplateElementKey.StaticText ? "Text" : null,
                Columns = DocumentTemplateRules.IsTable(key) ? DefaultColumns(key) : []
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