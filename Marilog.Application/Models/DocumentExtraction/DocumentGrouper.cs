using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Models.DocumentExtraction
{
    public static class DocumentGrouper
    {
        public static IReadOnlyList<ExtractedDocument> Group(
            IEnumerable<(int Page, IReadOnlyList<TextLine> Lines, ExtractedHeader Header)> pages)
        {
            var documents = new List<ExtractedDocument>();
            ExtractedDocument? current = null;

            foreach (var (page, lines, header) in pages)
            {
                // New document: a different number, or a page with its own total AND its own number or date.
                // A page with a total but no number and no date continues the current document (page 2 of a hire statement).
                var startsNew = current is null
                    || (header.DocNumber is not null && current.Header.DocNumber is not null &&
                        !string.Equals(header.DocNumber, current.Header.DocNumber, StringComparison.OrdinalIgnoreCase))
                    || (header.TotalAmount is not null && current.Header.TotalAmount is not null &&
                        (header.DocNumber is not null || header.DocDate is not null));

                if (startsNew)
                {
                    current = new ExtractedDocument { Header = header };
                    documents.Add(current);
                }
                else
                {
                    current!.Header = Merge(current.Header, header);
                }

                current.Pages.Add(page);
                current.Lines.AddRange(lines);
            }

            return documents;
        }

        private static ExtractedHeader Merge(ExtractedHeader a, ExtractedHeader b) => a with
        {
            DocNumber = a.DocNumber ?? b.DocNumber,
            DocDate = a.DocDate ?? b.DocDate,
            DocDue = a.DocDue ?? b.DocDue,
            TotalAmount = b.TotalAmount ?? a.TotalAmount,
            // The later page's total is the document's total; its currency comes with it.
            Currency = b.TotalAmount is not null
                ? b.Currency ?? a.Currency
                : a.Currency ?? b.Currency,
            Imo = a.Imo ?? b.Imo,
            PortText = a.PortText ?? b.PortText
        };
    }
}
