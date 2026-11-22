using Marilog.Kernel.Enums;

namespace Marilog.Kernel.Rules
{
    // Shared by the domain (validation), the PDF renderer and the designer page,
    // so all three agree on the page size. All values are in PDF points (1/72 inch).
    public static class DocumentTemplateRules
    {
        public const float MinFontSize = 4;
        public const float MaxFontSize = 72;
        public const float MinElementSize = 4;
        public const int MaxImageBytes = 2 * 1024 * 1024;

        public static (float Width, float Height) PageSize(TemplatePageSize size, bool landscape)
        {
            var (width, height) = size switch
            {
                TemplatePageSize.Letter => (612f, 792f),
                _ => (595f, 842f)   // A4, same as QuestPDF PageSizes.A4
            };

            return landscape ? (height, width) : (width, height);
        }

        public static bool IsTable(TemplateElementKey key)
            => key is TemplateElementKey.ItemsTable or TemplateElementKey.PaymentsTable;

        // The columns each table can show, in their natural order.
        public static TemplateColumnKey[] ColumnsFor(TemplateElementKey table) => table switch
        {
            TemplateElementKey.ItemsTable =>
            [
                TemplateColumnKey.ProductName, TemplateColumnKey.Quantity, TemplateColumnKey.Unit,
                TemplateColumnKey.UnitPrice, TemplateColumnKey.LineTotal
            ],
            TemplateElementKey.PaymentsTable =>
            [
                TemplateColumnKey.PaymentDate, TemplateColumnKey.PaymentMethod, TemplateColumnKey.PaymentReference,
                TemplateColumnKey.PaymentNote, TemplateColumnKey.PaidAmount
            ],
            _ => []
        };

        // Numbers are right-aligned in the designer and the PDF.
        public static bool IsNumberColumn(TemplateColumnKey key)
            => key is TemplateColumnKey.Quantity or TemplateColumnKey.UnitPrice
                or TemplateColumnKey.LineTotal or TemplateColumnKey.PaidAmount;

        public static string DefaultColumnTitle(TemplateColumnKey key) => key switch
        {
            TemplateColumnKey.ProductName => "Description",
            TemplateColumnKey.Quantity => "Qty",
            TemplateColumnKey.Unit => "Unit",
            TemplateColumnKey.UnitPrice => "Unit Price",
            TemplateColumnKey.LineTotal => "Amount",
            TemplateColumnKey.PaymentDate => "Date",
            TemplateColumnKey.PaymentMethod => "Method",
            TemplateColumnKey.PaymentReference => "Reference",
            TemplateColumnKey.PaymentNote => "Description",
            TemplateColumnKey.PaidAmount => "Amount",
            _ => key.ToString()
        };

        // "#RRGGBB" — what the browser colour picker gives and QuestPDF reads.
        public static bool IsHexColor(string? value)
            => value is { Length: 7 } && value[0] == '#' && value.Skip(1).All(Uri.IsHexDigit);
    }
}