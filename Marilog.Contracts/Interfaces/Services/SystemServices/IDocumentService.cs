using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;


namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IDocumentService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<DocumentResponse?>              GetByIdAsync(int id, bool treeView = false, CancellationToken ct = default);
        Task<DocumentResponse?>              GetWithItemsAsync(int id, bool treeView = false, CancellationToken ct = default);
        Task<DocumentResponse?>              GetWithPaymentsAsync(int id, bool treeView = false, CancellationToken ct = default);
        Task<DocumentResponse?>              GetFullAsync(int id, bool treeView = false, CancellationToken ct = default);
        Task<DocumentResponse?>              GetByNumberAsync(string docNumber, bool treeView = false, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetBySupplierAsync(int supplierId, bool treeView = false, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetByBuyerAsync(int buyerId, bool treeView = false, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetByVesselAsync(int vesselId, bool treeView = false, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetByTypeAsync(int docTypeId, bool treeView = false, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetUnpaidAsync(bool treeView = false, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetChildrenAsync(int parentDocumentId, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> GetAllAsTreeAsync(CancellationToken ct = default);
        Task<DocumentResponse?> GetTreeByDocumentIdAsync(int documentId, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> SearchAsync(string term, bool treeView = false, CancellationToken ct = default);
        // ── Commands ─────────────────────────────────────────────────────────────
        Task<DocumentResponse> CreateAsync(CreateDocumentRequest createDto, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentResponse>> CreateRangeAsync( IEnumerable<CreateDocumentRequest> commands, CancellationToken ct = default);

        Task           UpdateAsync(int documentId, UpdateDocumentRequest updateDto, CancellationToken ct = default);
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
        Task<PaymentResponse> UpdatePaymentAsync(int documentId, int paymentId, int swiftTransferId,
                                        decimal paidAmount, DateOnly paymentDate, CancellationToken ct = default);

        // ── Emails ────────────────────────────────────────────────────────────────
        Task LogEmailAsync(int documentId, string subject, string body,
                           IReadOnlyList<EmailParticipantResponse> participants,
                           EmailDirection direction = EmailDirection.Outbound,
                           CancellationToken ct = default);
        //---Reports------------------------------------------------------------------
        Task<DocumentReport> GetFilteredDocsReportAsync(DocumentFilterOptions options, CancellationToken ct = default);

        
    }
}
