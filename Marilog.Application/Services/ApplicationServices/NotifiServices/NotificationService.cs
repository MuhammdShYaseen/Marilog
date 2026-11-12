using Marilog.Application.Interfaces.UserIdentity;
using Marilog.Contracts.DTOs.Requests.NotificationDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.NotifiServices;
using Marilog.Domain.Entities.NotifiEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.NotifiServices
{
    public class NotificationService : INotificationService
    {
        private const int ListSize = 50;

        private readonly INotificationRepository _repo;
        private readonly ICurrentUserService _currentUser;

        public NotificationService(INotificationRepository repo, ICurrentUserService currentUser)
        {
            _repo = repo;
            _currentUser = currentUser;
        }

        private Guid UserId => _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Unable to identify the authenticated user.");

        // ── Follow ────────────────────────────────────────────────

        public Task<bool> IsFollowingAsync(EntityType type, int id, CancellationToken ct = default)
        {
            var userId = UserId;
            return _repo.Watches().AnyAsync(w => w.UserId == userId && w.EntityType == type && w.EntityId == id, ct);
        }

        public async Task<bool> FollowAsync(FollowRequest request, CancellationToken ct = default)
        {
            if (await IsFollowingAsync(request.EntityType, request.EntityId, ct)) return true;

            var guid = await _repo.GetEntityGuidAsync(request.EntityType, request.EntityId, ct);
            if (guid is null) return false;

            var title = string.IsNullOrWhiteSpace(request.Title)
                ? $"{request.EntityType} {request.EntityId}"
                : request.Title.Trim();

            _repo.Add(Watch.Create(UserId, request.EntityType, request.EntityId, guid.Value, title));
            await _repo.SaveChangesAsync(ct);
            return true;
        }

        public async Task UnfollowAsync(EntityType type, int id, CancellationToken ct = default)
        {
            var userId = UserId;
            var watch = await _repo.Watches()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.EntityType == type && w.EntityId == id, ct);
            if (watch is null) return;

            _repo.Remove(watch);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Notifications ─────────────────────────────────────────

        public async Task<IReadOnlyList<NotificationResponse>> GetMineAsync(IReadOnlyCollection<EntityType>? readableTypes = null, CancellationToken ct = default)
        {
            return await Mine(readableTypes)
                .OrderByDescending(n => n.Id)
                .Take(ListSize)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    Text = n.Text,
                    EntityType = n.EntityType,
                    EntityId = n.EntityId,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                })
                .ToListAsync(ct);
        }

        public Task<int> GetUnreadCountAsync(IReadOnlyCollection<EntityType>? readableTypes = null, CancellationToken ct = default)
            => Mine(readableTypes).CountAsync(n => n.ReadAt == null, ct);

        public async Task MarkReadAsync(long id, CancellationToken ct = default)
        {
            var userId = UserId;
            var notification = await _repo.Notifications()
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);
            if (notification is null) return;

            notification.MarkRead();
            await _repo.SaveChangesAsync(ct);
        }

        public async Task MarkAllReadAsync(CancellationToken ct = default)
        {
            var userId = UserId;
            var unread = await _repo.Notifications()
                .Where(n => n.UserId == userId && n.ReadAt == null)
                .ToListAsync(ct);

            foreach (var n in unread) n.MarkRead();
            await _repo.SaveChangesAsync(ct);
        }

        // The user's notifications, limited to the types they can read (null = no limit).
        private IQueryable<Notification> Mine(IReadOnlyCollection<EntityType>? readableTypes)
        {
            var userId = UserId;
            var query = _repo.Notifications().AsNoTracking().Where(n => n.UserId == userId);
            return readableTypes is null ? query : query.Where(n => readableTypes.Contains(n.EntityType));
        }
    }
}