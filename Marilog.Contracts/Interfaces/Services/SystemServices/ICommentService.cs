using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CommentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface ICommentService
    {
        // Queries
        Task<CommentResponse?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<PagedResponse<CommentResponse>> GetByUserIdAsync(Guid userId, PagedRequest request, CancellationToken ct = default);

        Task<IReadOnlyList<CommentResponse>> GetByEntityAsync(EntityType entityType, int entityId, DateTime? newSince = null, CancellationToken ct = default);

        // entityTypes / since are applied by the server; the client passes nothing.
        Task<IReadOnlyList<UnreadCommentCount>> GetUnreadSummaryAsync(List<EntityType>? entityTypes = null, DateTime? since = null, CancellationToken ct = default);

        Task<PagedResponse<CommentResponse>> GetAllAsync(PagedRequest request, CancellationToken ct = default);

        // Commands
        Task<CommentResponse> CreateAsync(CreateCommentRequest request, CancellationToken ct = default);

        Task<CommentResponse> UpdateAsync(int id, UpdateCommentRequest request, CancellationToken ct = default);

        Task MarkEntityReadAsync(EntityType entityType, int entityId, CancellationToken ct = default);

        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}