using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Ship.Services.Purchases
{
    public interface IPurchaseService
    {
        /// <summary>The vessel's documents dated from–to (inclusive), newest first.</summary>
        Task<IReadOnlyList<DocumentResponse>> GetAsync(DateOnly from, DateOnly to, CancellationToken ct = default);

        /// <summary>Creates the document (Expense, buyer = owner, vessel = this ship) and its payment. Returns the document Id.</summary>
        Task<int> CreateAsync(CreatePurchaseRequest request, CancellationToken ct = default);
    }
}