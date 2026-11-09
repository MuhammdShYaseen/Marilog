using Marilog.Application.Interfaces.UserIdentity;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CommentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _repo;
        private readonly ICurrentUserService _currentUser;
        public CommentService(IRepository<Comment> repository, ICurrentUserService currentUser)
        {
            _repo = repository;
            _currentUser = currentUser;
        }

        // ── Mapping ──────────────────────────────────────────────

        private static readonly Expression<Func<Comment, CommentResponse>> ToResponse =
            c => new CommentResponse
            {
                Id = c.Id,
                UserId = c.UserId,
                Text = c.Text,
                EntityType = c.EntityType,
                EntityId = c.EntityId,
                CreatedAt = c.CreatedAt,
                UserName = c.UserName,
                UpdatedAt = c.UpdatedAt
            };

        // ── Queries ──────────────────────────────────────────────

        public async Task<CommentResponse?> GetByIdAsync(
            int id,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PagedResponse<CommentResponse>> GetByUserIdAsync(Guid userId, PagedRequest request, CancellationToken ct = default)
        {
            var query = _repo.Query()
                .AsNoTracking()
                .Where(c => c.UserId == userId);

            return await GetPagedResultAsync(query, request, ct);
        }

        public async Task<IReadOnlyList<CommentResponse>> GetByEntityAsync(EntityType entityType, int entityId, DateTime? newSince = null, CancellationToken ct = default)
        {
            var userId = _currentUser.UserId;

            return await _repo.Query()
                .AsNoTracking()
                .Where(c => c.EntityType == entityType && c.EntityId == entityId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Text = c.Text,
                    EntityType = c.EntityType,
                    EntityId = c.EntityId,
                    CreatedAt = c.CreatedAt,
                    UserName = c.UserName,
                    UpdatedAt = c.UpdatedAt,
                    IsNew = c.UserId != userId
                            && (newSince == null || c.CreatedAt >= newSince)
                            && !c.Reads.Any(r => r.UserId == userId)
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<UnreadCommentCount>> GetUnreadSummaryAsync(List<EntityType>? entityTypes = null, DateTime? since = null, CancellationToken ct = default)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Unable to identify the authenticated user.");

            var query = _repo.Query()
                .AsNoTracking()
                .Where(c => c.EntityId != null
                         && c.UserId != userId
                         && !c.Reads.Any(r => r.UserId == userId));

            if (entityTypes is not null)
                query = query.Where(c => entityTypes.Contains(c.EntityType));

            if (since is not null)
                query = query.Where(c => c.CreatedAt >= since);

            return await query
                .GroupBy(c => new { c.EntityType, EntityId = c.EntityId!.Value })
                .Select(g => new UnreadCommentCount
                {
                    EntityType = g.Key.EntityType,
                    EntityId = g.Key.EntityId,
                    Count = g.Count()
                })
                .ToListAsync(ct);
        }

        public async Task MarkEntityReadAsync(EntityType entityType, int entityId, CancellationToken ct = default)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Unable to identify the authenticated user.");

            var unread = await _repo.Query()
                .Where(c => c.EntityType == entityType
                         && c.EntityId == entityId
                         && c.UserId != userId
                         && !c.Reads.Any(r => r.UserId == userId))
                .ToListAsync(ct);

            if (unread.Count == 0) return;

            foreach (var comment in unread)
                comment.MarkReadBy(userId);

            await _repo.SaveChangesAsync(ct);
        }

        public async Task<PagedResponse<CommentResponse>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
        {
            var query = _repo.Query()
                .AsNoTracking();

            return await GetPagedResultAsync(query, request, ct);
        }

        // ── Commands ─────────────────────────────────────────────

        public async Task<CommentResponse> CreateAsync(CreateCommentRequest request, CancellationToken ct = default)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("Unable to identify the authenticated user.");

            var comment = Comment.Create(request.Text, userId, _currentUser.UserName ?? string.Empty, request.EntityType, request.EntityId);

            await _repo.AddAsync(comment, ct);
            await _repo.SaveChangesAsync(ct);

            return await GetByIdAsync(comment.Id, ct)
                ?? throw new InvalidOperationException(
                    "Comment was created but could not be retrieved.");
        }

        public async Task<CommentResponse> UpdateAsync(int id, UpdateCommentRequest request, CancellationToken ct = default)
        {
          
             var comment = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"There is no comment with ID: {id}");

            EnsureAuthor(comment);

            comment.UpdateText(request.Text);

            await _repo.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("Comment was updated but could not be retrieved.");
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var comment = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"There is no comment with ID: {id}");
            EnsureAuthor(comment);
            _repo.HardDelete(comment);

            await _repo.SaveChangesAsync(ct);
        }

        // ── Private Helpers ──────────────────────────────────────

        private static async Task<PagedResponse<CommentResponse>> GetPagedResultAsync(IQueryable<Comment> query, PagedRequest request, CancellationToken ct)
        {
            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ToResponse)
                .ToListAsync(ct);

            return new PagedResponse<CommentResponse>
            {
                Items = items,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
            };
        }


        private void EnsureAuthor(Comment comment)
        {
            if (comment.UserId != _currentUser.UserId)
                throw new InvalidOperationException("You can only edit or delete your own comments.");
        }
    }
}