using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface IDocumentService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Document?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Document?>              GetWithItemsAsync(int id, CancellationToken ct = default);
        Task<Document?>              GetWithPaymentsAsync(int id, CancellationToken ct = default);
        Task<Document?>              GetFullAsync(int id, CancellationToken ct = default);
        Task<Document?>              GetByNumberAsync(string docNumber, CancellationToken ct = default);
        Task<IReadOnlyList<Document>> GetBySupplierAsync(int supplierId, CancellationToken ct = default);
        Task<IReadOnlyList<Document>> GetByBuyerAsync(int buyerId, CancellationToken ct = default);
        Task<IReadOnlyList<Document>> GetByVesselAsync(int vesselId, CancellationToken ct = default);
        Task<IReadOnlyList<Document>> GetByTypeAsync(int docTypeId, CancellationToken ct = default);
        Task<IReadOnlyList<Document>> GetUnpaidAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Document>> GetChildrenAsync(int parentDocumentId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Document> CreateAsync(string docNumber, int docTypeId, DateOnly docDate,
                                   int currencyId, decimal totalAmount,
                                   int? supplierId = null, int? buyerId = null,
                                   int? vesselId = null, int? portId = null,
                                   int? parentDocumentId = null,
                                   string? reference = null, string? filePath = null,
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
        Task<DocumentItem> AddItemAsync(int documentId, string productName, decimal quantity,
                                        decimal unitPrice, string? unit = null,
                                        CancellationToken ct = default);
        Task               UpdateItemAsync(int documentId, int itemId, string productName,
                                           decimal quantity, decimal unitPrice,
                                           string? unit = null, CancellationToken ct = default);
        Task               RemoveItemAsync(int documentId, int itemId, CancellationToken ct = default);

        // ── Payments ──────────────────────────────────────────────────────────────
        Task<Payment> AddPaymentAsync(int documentId, int swiftTransferId,
                                      decimal paidAmount, DateOnly paymentDate,
                                      CancellationToken ct = default);

        // ── Emails ────────────────────────────────────────────────────────────────
        Task LogEmailAsync(int documentId, string subject, string body,
                           IReadOnlyList<EmailParticipantData> participants,
                           EmailDirection direction = EmailDirection.Outbound,
                           CancellationToken ct = default);
    }
}
