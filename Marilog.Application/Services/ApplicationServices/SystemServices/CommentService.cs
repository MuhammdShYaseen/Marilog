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
                CreatedAt = c.CreatedAt
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

        public async Task<IReadOnlyList<CommentResponse>> GetByEntityAsync(EntityType entityType, int entityId, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(c => c.EntityType == entityType && c.EntityId == entityId)
                .OrderBy(c => c.CreatedAt)
                .Select(ToResponse)
                .ToListAsync(ct);
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

            var comment = Comment.Create(request.Text, userId, request.EntityType, request.EntityId);

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
                PageSize = request.PageSize
            };
        }


        private void EnsureAuthor(Comment comment)
        {
            if (comment.UserId != _currentUser.UserId)
                throw new InvalidOperationException("You can only edit or delete your own comments.");
        }
    }
}