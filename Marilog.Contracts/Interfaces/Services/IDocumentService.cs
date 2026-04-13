using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Enums;

namespace Marilog.Contracts.Interfaces.Services
{
    public interface IDocumentService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<DocumentResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<DocumentResponse?>              GetWithItemsAsync(int id, CancellationToken ct = default);
        Task<DocumentResponse?>              GetWithPaymentsAsync(int id, CancellationToken ct = default);
        Task<DocumentResponse?>              GetFullAsync(int id, CancellationToken ct = default);
        Task<DocumentResponse?>              GetByNumberAsync(string docNumber, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetBySupplierAsync(int supplierId, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetByBuyerAsync(int buyerId, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetByVesselAsync(int vesselId, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetByTypeAsync(int docTypeId, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetUnpaidAsync(CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetChildrenAsync(int parentDocumentId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<DocumentResponse> CreateAsync(string docNumber, int docTypeId, DateOnly docDate,
                                   int currencyId, decimal totalAmount,
                                   int? supplierId = null, int? buyerId = null,
                                   int? vesselId = null, int? portId = null,
                                   int? parentDocumentId = null,
                                   string? reference = null, string? filePath = null,
                                   CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> CreateRangeAsync(
                                                IEnumerable<CreateDocumentRequest> commands,
                                                CancellationToken ct = default);

        Task           UpdateAsync(int id, int docTypeId, DateOnly docDate,
                                   int currencyId, decimal totalAmount,
                                   int? supplierId = null, int? buyerId = null,
                                   int? vesselId = null, int? portId = null,
                                   string? reference = null, string? filePath = null,
                                   CancellationToken ct = default);
        Task           LinkToParentAsync(int id, int parentDocumentId, CancellationToken ct = default);
        Task           UnlinkFromParentAsync(int id, CancellationToken ct = default);
        Task           ActivateAsync(int id, CancellationToken ct = default);
        Task           DeactivateAsync(int id, CancellationToken ct = default);
        Task           DeleteAsync(int id, CancellationToken ct = default);

        // ── Items ─────────────────────────────────────────────────────────────────
        Task<DocumentItemResponse> AddItemAsync(int documentId, string productName, decimal quantity,
                                        decimal unitPrice, string? unit = null,
                                        CancellationToken ct = default);

        Task<IReadOnlyList<DocumentItemResponse>> AddItemsRangeAsync(int documentId, IEnumerable<AddDocumentItemRequest> commands, CancellationToken ct = default);

        Task               UpdateItemAsync(int documentId, int itemId, string productName,
                                           decimal quantity, decimal unitPrice,
                                           string? unit = null, CancellationToken ct = default);
        Task               RemoveItemAsync(int documentId, int itemId, CancellationToken ct = default);

        // ── Payments ──────────────────────────────────────────────────────────────
        Task<PaymentResponse> AddPaymentAsync(int documentId, int swiftTransferId,
                                      decimal paidAmount, DateOnly paymentDate,
                                      CancellationToken ct = default);

        // ── Emails ────────────────────────────────────────────────────────────────
        Task LogEmailAsync(int documentId, string subject, string body,
                           IReadOnlyList<EmailParticipantData> participants,
                           EmailDirection direction = EmailDirection.Outbound,
                           CancellationToken ct = default);
        //---Reports------------------------------------------------------------------
        Task<DocumentReport> GetFilteredDocsReportAsync(DocumentFilterOptions options, CancellationToken ct = default);
    }
}
