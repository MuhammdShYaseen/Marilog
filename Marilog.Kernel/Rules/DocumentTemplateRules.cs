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

        public static string DefaultColumnTitle(TemplateColumnKey key) => key switch
        {
            TemplateColumnKey.ProductName => "Description",
            TemplateColumnKey.Quantity => "Qty",
            TemplateColumnKey.Unit => "Unit",
            TemplateColumnKey.UnitPrice => "Unit Price",
            TemplateColumnKey.LineTotal => "Amount",
            _ => key.ToString()
        };
    }
}