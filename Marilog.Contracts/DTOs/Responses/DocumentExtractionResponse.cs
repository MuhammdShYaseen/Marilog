
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed record ExtractedHeader(
        string? Title,
        string? DocNumber,
        DateOnly? DocDate,
        DateOnly? DocDue,
        decimal? TotalAmount,
        string? Currency,
        string? Imo,
        string? PortText);

    public sealed record ExtractionDebugPage(
        int Number,
        bool HasTextLayer,
        IReadOnlyList<string> Lines,
        ExtractedHeader? Header);

    public sealed record ExtractionFile(string FileName, byte[] Content);

    public sealed record ExtractionResult(
        string FileName,
        IReadOnlyList<int> Pages,
        CreateDocumentRequest Payload,
        IReadOnlyList<string> Unresolved,
        IReadOnlyList<string> Warnings);

    public sealed record LookupItem(int Id, string Name);

    public sealed record ExtractionLookups(
    IReadOnlyList<LookupItem> DocTypes,
    IReadOnlyList<LookupItem> Currencies,
    IReadOnlyList<LookupItem> Companies,
    IReadOnlyList<LookupItem> Vessels,
    IReadOnlyList<LookupItem> Ports);

    public sealed record ExtractionResponse(
        IReadOnlyList<ExtractionResult> Documents,
        IReadOnlyList<string> Notes,
        ExtractionLookups Lookups);
}
