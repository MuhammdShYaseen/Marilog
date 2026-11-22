using Marilog.Application.Interfaces.DocumentTemplates;
using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Rules;
using QuestPDF.Drawing.Exceptions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System.Globalization;
using PdfDocument = QuestPDF.Fluent.Document;

namespace Marilog.Infrastructure.Services.Qpdf
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Renders a user-designed DocumentTemplate with a Document's data.
    //
    //  Every element sits in the page Background at its own X/Y/Width/Height (points).
    //  The items table is the page Content: the page margins are its box, so when
    //  the items run out of room QuestPDF adds a page with the same background and
    //  continues the table in the same box, repeating its header row.
    //  PageScope decides whether an element prints on every page, the first or the last.
    //  The payments table is a fixed box like a field: it doesn't continue on a new page.
    //  The page number is a text box whose numbers QuestPDF fills in on every page.
    // ═══════════════════════════════════════════════════════════════════════
    public sealed class DocumentTemplateRenderer : IDocumentTemplateRenderer
    {
        private const string FontsRoute = "api/DocumentTemplates/fonts/";
        private const float BoxPadding = 2;      // inner padding of a bordered box — the designer uses the same
        private static readonly string FontsFolder = Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts");
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        // Read once: QuestPDF also registers the folder's fonts once at startup, so a new font needs a restart.
        private static readonly Lazy<IReadOnlyList<TemplateFontResponse>> Fonts = new(LoadFonts);

        // ── Fonts ────────────────────────────────────────────────

        public IReadOnlyList<TemplateFontResponse> GetFonts() => Fonts.Value;

        public string? GetFontFilePath(string fileName)
        {
            // Only names from the list are served, so a request can never reach outside the folder.
            var font = Fonts.Value.FirstOrDefault(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            return font is null ? null : Path.Combine(FontsFolder, font.FileName);
        }

        private static IReadOnlyList<TemplateFontResponse> LoadFonts()
        {
            if (!Directory.Exists(FontsFolder)) return [];

            var fonts = new List<TemplateFontResponse>();

            foreach (var path in Directory.EnumerateFiles(FontsFolder).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                var extension = Path.GetExtension(path);
                if (!extension.Equals(".ttf", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".otf", StringComparison.OrdinalIgnoreCase))
                    continue;

                using var typeface = SKTypeface.FromFile(path);
                if (typeface is null) continue;

                var fileName = Path.GetFileName(path);
                fonts.Add(new TemplateFontResponse
                {
                    Family = typeface.FamilyName,
                    Weight = typeface.FontWeight,
                    IsItalic = typeface.FontSlant != SKFontStyleSlant.Upright,
                    FileName = fileName,
                    Url = FontsRoute + Uri.EscapeDataString(fileName)
                });
            }

            return fonts;
        }

        // ── Render ───────────────────────────────────────────────

        public byte[] Render(DocumentTemplateResponse template, DocumentTemplateData? input, IReadOnlyDictionary<int, byte[]> images)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var data = input ?? SampleData();
            var (pageWidth, pageHeight) = DocumentTemplateRules.PageSize(template.PageSize, template.IsLandscape);

            // The chosen font first; the bundled Noto fonts after it cover Arabic and any missing glyph.
            string[] fonts = [template.FontFamily, .. PdfFonts.Families];

            var table = template.Elements.FirstOrDefault(e => e.Key == TemplateElementKey.ItemsTable);
            var boxes = template.Elements
                .Select((e, index) => (Element: e, Image: images.GetValueOrDefault(index)))
                .Where(b => b.Element.Key != TemplateElementKey.ItemsTable)
                .ToList();

            try
            {
                return PdfDocument.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(pageWidth, pageHeight, Unit.Point);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontFamily(fonts).FontSize(10).FontColor(Colors.Black));

                        page.Background().Layers(layers =>
                        {
                            layers.PrimaryLayer();
                            foreach (var box in boxes)
                                layers.Layer().Element(c => ComposeBox(c, box.Element, box.Image, data));
                        });

                        if (table is null) return;

                        page.MarginLeft(table.X);
                        page.MarginTop(table.Y);
                        page.MarginRight(Math.Max(0, pageWidth - table.X - table.Width));
                        page.MarginBottom(Math.Max(0, pageHeight - table.Y - table.Height));
                        page.Content().Element(c => ComposeTable(c, table, data.Document.Items, ItemCell));
                    });
                }).GeneratePdf();
            }
            catch (DocumentLayoutException)
            {
                throw new InvalidOperationException(
                    "The template layout doesn't fit. Usually one item line is taller than the items table box: " +
                    "make the items box taller or its font smaller.");
            }
        }

        // ── Boxes (fields, fixed text, images) ───────────────────

        private static void ComposeBox(IContainer container, DocumentTemplateElementDto e, byte[]? image, DocumentTemplateData data)
        {
            switch (e.PageScope)
            {
                case TemplatePageScope.First:
                    DrawBox(container.ShowIf(ctx => ctx.PageNumber == 1), e, image, data);
                    break;

                // Not ShowIf: QuestPDF keeps the drawing from its first pass, when the page count is still unknown,
                // so a "last page" ShowIf never shows. A dynamic component is drawn again in the final pass.
                case TemplatePageScope.Last:
                    container.Dynamic(new LastPageOnly(c => DrawBox(c, e, image, data)));
                    break;

                default:
                    DrawBox(container, e, image, data);
                    break;
            }
        }

        private static void DrawBox(IContainer container, DocumentTemplateElementDto e, byte[]? image, DocumentTemplateData data)
        {
            var area = container
                .PaddingLeft(e.X)
                .PaddingTop(e.Y)
                .Width(e.Width)
                .Height(e.Height);

            // Its colours style the header row, like the items table. No payments = nothing printed.
            // Payments are few; ScaleToFit only shrinks the table when there are more than the box holds.
            if (e.Key == TemplateElementKey.PaymentsTable)
            {
                if (data.Document.Payments.Count > 0)
                    area.ScaleToFit().Element(c => ComposeTable(c, e, data.Document.Payments, PaymentCell));
                return;
            }

            var box = area
                .Element(x => e.Background is { } fill ? x.Background(Color.FromHex(fill)) : x)
                .Element(x => e.Border ? x.Border(0.75f).BorderColor(TextColor(e)).Padding(BoxPadding) : x);

            if (e.Key == TemplateElementKey.Image)
            {
                // No picture (e.g. its file was deleted): leave the box empty instead of failing the export.
                if (image is { Length: > 0 })
                    box.Image(image).FitArea();
                return;
            }

            // QuestPDF writes the numbers itself on each page.
            if (e.Key == TemplateElementKey.PageNumber)
            {
                box.ScaleToFit().Text(text =>
                {
                    AlignText(text, e.Align);
                    Style(text.Span("Page "), e);
                    Style(text.CurrentPageNumber(), e);
                    Style(text.Span(" of "), e);
                    Style(text.TotalPages(), e);
                });
                return;
            }

            var value = e.Key == TemplateElementKey.StaticText ? e.Text ?? string.Empty : FieldValue(e.Key, data);

            // Normally the text fits the box as designed; ScaleToFit only shrinks it when a value is longer than expected.
            box.ScaleToFit().Text(text =>
            {
                AlignText(text, e.Align);
                Style(text.Span(value), e);
            });
        }

        private static void Style(TextSpanDescriptor span, DocumentTemplateElementDto e)
        {
            span.FontSize(e.FontSize).FontColor(TextColor(e));
            if (e.Bold) span.Bold();
        }

        // Black unless the user picked a colour.
        private static Color TextColor(DocumentTemplateElementDto e)
            => e.Color is { } color ? Color.FromHex(color) : Colors.Black;

        // Draws its content on the last page only. TotalPages is known in QuestPDF's final pass.
        private sealed class LastPageOnly(Action<IContainer> content) : IDynamicComponent
        {
            public DynamicComponentComposeResult Compose(QuestPDF.Elements.DynamicContext context) => new()
            {
                Content = context.PageNumber == context.TotalPages
                    ? context.CreateElement(content)
                    : context.CreateElement(_ => { }),
                HasMoreContent = false
            };
        }

        // ── Tables (items, payments) ─────────────────────────────

        private static void ComposeTable<T>(IContainer container, DocumentTemplateElementDto e, List<T> rows,
            Func<TemplateColumnKey, T, string> cell)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    foreach (var column in e.Columns)
                        columns.RelativeColumn(column.Width);
                });

                // Repeated at the top of every page the items table continues on. The element's colours style this row.
                table.Header(header =>
                {
                    foreach (var column in e.Columns)
                    {
                        header.Cell()
                            .Element(c => e.Background is { } fill ? c.Background(Color.FromHex(fill)) : c)
                            .Element(c => e.Border ? c.Border(0.5f).BorderColor(Colors.Black) : c.BorderBottom(0.75f).BorderColor(Colors.Black))
                            .PaddingVertical(3).PaddingHorizontal(4)
                            .Text(text =>
                            {
                                AlignColumn(text, column.Key);
                                text.Span(column.Title ?? DocumentTemplateRules.DefaultColumnTitle(column.Key))
                                    .FontSize(e.FontSize).Bold().FontColor(TextColor(e));
                            });
                    }
                });

                foreach (var row in rows)
                {
                    foreach (var column in e.Columns)
                    {
                        table.Cell()
                            .Element(c => e.Border ? c.Border(0.5f).BorderColor(Colors.Black) : c.BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten1))
                            .PaddingVertical(3).PaddingHorizontal(4)
                            .Text(text =>
                            {
                                AlignColumn(text, column.Key);
                                var span = text.Span(cell(column.Key, row)).FontSize(e.FontSize);
                                if (e.Bold) span.Bold();
                            });
                    }
                }
            });
        }

        // ── Values ───────────────────────────────────────────────

        private static string FieldValue(TemplateElementKey key, DocumentTemplateData data)
        {
            var d = data.Document;
            return key switch
            {
                TemplateElementKey.DocNumber => d.DocNumber,
                TemplateElementKey.DocType => d.DocTypeName ?? string.Empty,
                TemplateElementKey.DocDate => FormatDate(d.DocDate),
                TemplateElementKey.DocDue => d.DocDue is { } due ? FormatDate(due) : string.Empty,
                TemplateElementKey.Reference => d.Reference ?? string.Empty,
                TemplateElementKey.Supplier => d.SupplierName ?? string.Empty,
                TemplateElementKey.Buyer => d.BuyerName ?? string.Empty,
                TemplateElementKey.Vessel => d.VesselName ?? string.Empty,
                TemplateElementKey.Port => d.PortName ?? string.Empty,
                TemplateElementKey.Voyage => d.VoyageNumber ?? string.Empty,
                TemplateElementKey.Currency => d.CurrencyCode,
                TemplateElementKey.TotalAmount => FormatMoney(d.TotalAmount),
                TemplateElementKey.AdjustmentsTotal => FormatMoney(d.AdjustmentsTotal),
                TemplateElementKey.NetAmount => FormatMoney(d.NetAmount),
                TemplateElementKey.TotalPaid => FormatMoney(d.TotalPaid),
                TemplateElementKey.RemainingBalance => FormatMoney(d.RemainingBalance),
                TemplateElementKey.SupplierAddress => data.Supplier?.Address ?? string.Empty,
                TemplateElementKey.SupplierCountry => data.Supplier?.Country ?? string.Empty,
                TemplateElementKey.SupplierRegistrationNumber => data.Supplier?.RegistrationNumber ?? string.Empty,
                TemplateElementKey.SupplierContactName => data.Supplier?.ContactName ?? string.Empty,
                TemplateElementKey.BuyerAddress => data.Buyer?.Address ?? string.Empty,
                TemplateElementKey.BuyerCountry => data.Buyer?.Country ?? string.Empty,
                TemplateElementKey.BuyerRegistrationNumber => data.Buyer?.RegistrationNumber ?? string.Empty,
                TemplateElementKey.BuyerContactName => data.Buyer?.ContactName ?? string.Empty,
                TemplateElementKey.SupplierEmail => data.Supplier?.Email ?? string.Empty,
                TemplateElementKey.SupplierPhone => data.Supplier?.Phone ?? string.Empty,
                TemplateElementKey.SupplierBankDetails => BankDetails(data.Supplier?.BankAccount),
                TemplateElementKey.BuyerEmail => data.Buyer?.Email ?? string.Empty,
                TemplateElementKey.BuyerPhone => data.Buyer?.Phone ?? string.Empty,
                TemplateElementKey.BuyerBankDetails => BankDetails(data.Buyer?.BankAccount),
                TemplateElementKey.AmountInWords => AmountInWords.Write(d.NetAmount, d.CurrencyCode, data.CurrencyName),
                _ => string.Empty
            };
        }
        // One line each; lines with no value are left out.
        private static string BankDetails(TemplateBankAccount? account)
        {
            if (account is null) return string.Empty;

            string?[] lines =
            [
                $"Bank: {account.BankName}",
                string.IsNullOrWhiteSpace(account.AccountHolderName) ? null : $"Account name: {account.AccountHolderName}",
                $"IBAN: {account.Iban}",
                string.IsNullOrWhiteSpace(account.SwiftCode) ? null : $"SWIFT: {account.SwiftCode}"
            ];

            return string.Join("\n", lines.Where(l => l is not null));
        }
        private static string ItemCell(TemplateColumnKey key, DocumentItemResponse item) => key switch
        {
            TemplateColumnKey.ProductName => item.ProductName,
            TemplateColumnKey.Quantity => item.Quantity.ToString("#,##0.###", Invariant),
            TemplateColumnKey.Unit => item.Unit ?? string.Empty,
            TemplateColumnKey.UnitPrice => FormatMoney(item.UnitPrice),
            TemplateColumnKey.LineTotal => FormatMoney(item.LineTotal),
            _ => string.Empty
        };

        private static string PaymentCell(TemplateColumnKey key, PaymentResponse payment) => key switch
        {
            TemplateColumnKey.PaymentDate => FormatDate(payment.PaymentDate),
            TemplateColumnKey.PaymentMethod => MethodLabel(payment.PaymentMethod),
            TemplateColumnKey.PaymentReference => payment.SwiftTransfer?.SwiftReference
                                                  ?? payment.SwiftTransfer?.PaymentReference ?? string.Empty,
            TemplateColumnKey.PaymentNote => payment.Discreption ?? string.Empty,
            TemplateColumnKey.PaidAmount => FormatMoney(payment.PaidAmount),
            _ => string.Empty
        };

        private static string MethodLabel(PaymentMethod method) => method switch
        {
            PaymentMethod.CashOnBoard => "Cash on board",
            PaymentMethod.CashAtOffice => "Cash at office",
            PaymentMethod.BankTransfer => "Bank transfer",
            PaymentMethod.Swift => "SWIFT",
            PaymentMethod.Cash => "Cash",
            PaymentMethod.Cheque => "Cheque",
            PaymentMethod.CreditBalance => "Credit balance",
            PaymentMethod.CreditCard => "Credit card",
            _ => "Other"
        };

        private static string FormatDate(DateOnly date) => date.ToString("dd MMM yyyy", Invariant);

        private static string FormatMoney(decimal value) => value.ToString("#,##0.00", Invariant);

        private static void AlignText(TextDescriptor text, TemplateTextAlign align)
        {
            switch (align)
            {
                case TemplateTextAlign.Center: text.AlignCenter(); break;
                case TemplateTextAlign.Right: text.AlignRight(); break;
                default: text.AlignLeft(); break;
            }
        }

        // Numbers right, text left.
        private static void AlignColumn(TextDescriptor text, TemplateColumnKey key)
        {
            if (DocumentTemplateRules.IsNumberColumn(key))
                text.AlignRight();
            else
                text.AlignLeft();
        }

        // Fills the fields when the designer previews without picking a real document.
        private static DocumentTemplateData SampleData() => new()
        {
            Document = SampleDocument(),
            Supplier = new TemplateCompany("Street 1, Business Bay, Dubai", "United Arab Emirates", "REG-0001", "Supplier Contact",
                "accounts@supplier.com", "+971 4 000 0000",
                new TemplateBankAccount("Sample Bank", "Supplier Company", "AE00 0000 0000 0000 0000 000", "SAMPAEAD")),
            Buyer = new TemplateCompany("Street 2, Port Area, Monrovia", "Liberia", "REG-0002", "Buyer Contact",
                "accounts@buyer.com", "+231 00 000 000",
                new TemplateBankAccount("Buyer Bank", "Buyer Company", "LR00 0000 0000 0000 0000", "BUYBLRLM")),
            CurrencyName = "US Dollar"
        };

        private static DocumentResponse SampleDocument()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            return new DocumentResponse
            {
                DocNumber = "INV-0001",
                DocTypeName = "Invoice",
                DocDate = today,
                DocDue = today.AddDays(30),
                Reference = "REF-0001",
                SupplierName = "Supplier Company",
                BuyerName = "Buyer Company",
                VesselName = "MV SAMPLE",
                PortName = "Port Name",
                VoyageNumber = "V-001",
                CurrencyCode = "USD",
                TotalAmount = 12_500m,
                AdjustmentsTotal = -500m,
                NetAmount = 12_000m,
                TotalPaid = 7_000m,
                RemainingBalance = 5_000m,
                Payments =
                [
                    new PaymentResponse
                    {
                        PaymentDate = today.AddDays(-10), PaymentMethod = PaymentMethod.Swift, PaidAmount = 5_000m,
                        Discreption = "Advance", SwiftTransfer = new SwiftTransferResponse { SwiftReference = "SWIFT-0001" }
                    },
                    new PaymentResponse
                    {
                        PaymentDate = today.AddDays(-2), PaymentMethod = PaymentMethod.CashOnBoard, PaidAmount = 2_000m,
                        Discreption = "Cash to Master", SwiftTransfer = null
                    }
                ],
                Items =
                [
                    new DocumentItemResponse { ProductName = "Freight", Quantity = 1, Unit = "LS", UnitPrice = 10_000m, LineTotal = 10_000m },
                    new DocumentItemResponse { ProductName = "Demurrage", Quantity = 2.5m, Unit = "Days", UnitPrice = 800m, LineTotal = 2_000m },
                    new DocumentItemResponse { ProductName = "Port expenses", Quantity = 1, Unit = "LS", UnitPrice = 500m, LineTotal = 500m }
                ]
            };
        }
    }
}