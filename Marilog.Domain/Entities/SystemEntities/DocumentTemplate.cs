using Marilog.Domain.Common;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Rules;

namespace Marilog.Domain.Entities.SystemEntities
{
    // A user-designed page for exporting a Document as PDF.
    // Knows nothing about Document: the export picks a template and a document and renders them together.
    // All positions and sizes are in PDF points (1/72 inch), measured from the page's top-left corner.
    public class DocumentTemplate : Entity
    {
        public string Name { get; private set; } = null!;
        public TemplatePageSize PageSize { get; private set; }
        public bool IsLandscape { get; private set; }
        public string FontFamily { get; private set; } = null!;
        public List<DocumentTemplateElement> Elements { get; private set; } = [];

        private DocumentTemplate() { }

        public static DocumentTemplate Create(string name, TemplatePageSize pageSize, bool isLandscape,
            string fontFamily, List<DocumentTemplateElement> elements)
        {
            var template = new DocumentTemplate();
            template.Update(name, pageSize, isLandscape, fontFamily, elements);
            return template;
        }

        // The designer always sends the whole page, so an update replaces everything at once.
        public void Update(string name, TemplatePageSize pageSize, bool isLandscape,
            string fontFamily, List<DocumentTemplateElement> elements)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Template name is required.", nameof(name));

            if (name.Trim().Length > 100)
                throw new ArgumentException("Template name must not exceed 100 characters.", nameof(name));

            if (!Enum.IsDefined(pageSize))
                throw new ArgumentException("Unknown page size.", nameof(pageSize));

            if (string.IsNullOrWhiteSpace(fontFamily))
                throw new ArgumentException("Font is required.", nameof(fontFamily));

            elements ??= [];

            var (pageWidth, pageHeight) = DocumentTemplateRules.PageSize(pageSize, isLandscape);
            foreach (var element in elements)
                element.Validate(pageWidth, pageHeight);

            if (elements.Count(e => e.Key == TemplateElementKey.ItemsTable) > 1)
                throw new ArgumentException("A template can have only one items table.", nameof(elements));

            Name = name.Trim();
            PageSize = pageSize;
            IsLandscape = isLandscape;
            FontFamily = fontFamily.Trim();
            Elements = elements;
            Touch();
        }
    }

    // Owned by DocumentTemplate, stored as JSON.
    public class DocumentTemplateElement
    {
        public TemplateElementKey Key { get; private set; }
        public TemplatePageScope PageScope { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float FontSize { get; private set; }
        public bool Bold { get; private set; }
        public TemplateTextAlign Align { get; private set; }
        public bool Border { get; private set; }

        // "#RRGGBB". Text and border colour, null = black. For the items table: the header row text.
        public string? Color { get; private set; }

        // "#RRGGBB" fill, null = transparent. For the items table: the header row background.
        public string? Background { get; private set; }

        // StaticText only
        public string? Text { get; private set; }

        // Image only: a data URL ("data:image/png;base64,...") — the browser shows it as is, the PDF decodes it.
        [AuditIgnore]
        public string? ImageDataUrl { get; private set; }

        // ItemsTable only
        public List<DocumentTemplateColumn> Columns { get; private set; } = [];

        private DocumentTemplateElement() { }

        public static DocumentTemplateElement Create(TemplateElementKey key, TemplatePageScope pageScope,
            float x, float y, float width, float height, float fontSize, bool bold,
            TemplateTextAlign align, bool border, string? color, string? background,
            string? text, string? imageDataUrl, List<DocumentTemplateColumn>? columns)
            => new()
            {
                Key = key,
                PageScope = pageScope,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                FontSize = fontSize,
                Bold = bold,
                Align = align,
                Border = border,
                Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim(),
                Background = string.IsNullOrWhiteSpace(background) ? null : background.Trim(),
                Text = key == TemplateElementKey.StaticText ? text : null,
                ImageDataUrl = key == TemplateElementKey.Image ? imageDataUrl?.Trim() : null,
                Columns = key == TemplateElementKey.ItemsTable ? columns ?? [] : []
            };

        internal void Validate(float pageWidth, float pageHeight)
        {
            if (!Enum.IsDefined(Key))
                throw new ArgumentException($"Unknown template element: {(int)Key}.");

            if (!Enum.IsDefined(PageScope) || !Enum.IsDefined(Align))
                throw new ArgumentException($"{Key}: invalid page scope or alignment.");

            if (Width < DocumentTemplateRules.MinElementSize || Height < DocumentTemplateRules.MinElementSize)
                throw new ArgumentException($"{Key}: width and height must be at least {DocumentTemplateRules.MinElementSize} pt.");

            // Half a point of tolerance for rounding in the browser.
            if (X < 0 || Y < 0 || X + Width > pageWidth + 0.5f || Y + Height > pageHeight + 0.5f)
                throw new ArgumentException($"{Key} is outside the page.");

            if (FontSize < DocumentTemplateRules.MinFontSize || FontSize > DocumentTemplateRules.MaxFontSize)
                throw new ArgumentException($"{Key}: font size must be between {DocumentTemplateRules.MinFontSize} and {DocumentTemplateRules.MaxFontSize}.");

            if (Color is not null && !DocumentTemplateRules.IsHexColor(Color))
                throw new ArgumentException($"{Key}: colour must look like #RRGGBB.");

            if (Background is not null && !DocumentTemplateRules.IsHexColor(Background))
                throw new ArgumentException($"{Key}: background colour must look like #RRGGBB.");

            switch (Key)
            {
                case TemplateElementKey.StaticText when string.IsNullOrWhiteSpace(Text):
                    throw new ArgumentException("Fixed text can't be empty.");

                case TemplateElementKey.Image:
                    ValidateImage();
                    break;

                case TemplateElementKey.ItemsTable:
                    ValidateColumns();
                    break;
            }
        }

        // Returns the raw image bytes of an Image element.
        public byte[] GetImageBytes()
        {
            var comma = ImageDataUrl?.IndexOf(',') ?? -1;
            if (comma < 0)
                throw new ArgumentException("Image data is not a valid data URL.");

            try
            {
                return Convert.FromBase64String(ImageDataUrl![(comma + 1)..]);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Image data is not valid base64.");
            }
        }

        private void ValidateImage()
        {
            if (string.IsNullOrWhiteSpace(ImageDataUrl)
                || !ImageDataUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)
                || !ImageDataUrl.Contains(";base64,", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Image data must be a base64 image data URL.");

            if (GetImageBytes().Length > DocumentTemplateRules.MaxImageBytes)
                throw new ArgumentException($"Image must not exceed {DocumentTemplateRules.MaxImageBytes / 1024 / 1024} MB.");
        }

        private void ValidateColumns()
        {
            if (Columns.Count == 0)
                throw new ArgumentException("The items table needs at least one column.");

            if (Columns.Any(c => !Enum.IsDefined(c.Key) || c.Width <= 0))
                throw new ArgumentException("Items table: every column needs a known field and a width above zero.");

            if (Columns.GroupBy(c => c.Key).Any(g => g.Count() > 1))
                throw new ArgumentException("Items table: a column can appear only once.");
        }
    }

    // Owned by DocumentTemplateElement (items table only). Width is relative to the other columns.
    public class DocumentTemplateColumn
    {
        public TemplateColumnKey Key { get; private set; }
        public float Width { get; private set; }
        public string Title { get; private set; } = null!;

        private DocumentTemplateColumn() { }

        public static DocumentTemplateColumn Create(TemplateColumnKey key, float width, string? title)
            => new()
            {
                Key = key,
                Width = width,
                Title = string.IsNullOrWhiteSpace(title) ? DocumentTemplateRules.DefaultColumnTitle(key) : title.Trim()
            };
    }
}