using Marilog.Application.Interfaces.UserIdentity;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.AuditDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.AuditLog;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Marilog.Application.Services.ApplicationServices.AuditLog
{
    public sealed class AuditLogService : IAuditLogService
    {
        // Hard floor: no setting can go below this

        private readonly IAuditLogRepository _repo;
        private readonly IAuditSettingsStore _settings;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IAuditLogRepository repo, IAuditSettingsStore settings, ICurrentUserService currentUser, ILogger<AuditLogService> logger)
        {
            _repo = repo;
            _settings = settings;
            _currentUser = currentUser;
            _logger = logger;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<PagedResponse<AuditLogListItem>> GetPagedAsync(
            PagedRequest paging, AuditLogFilter filter, CancellationToken ct = default)
        {
            var q = _repo.Query().AsNoTracking();

            if (filter.EntityType.HasValue)
            {
                var entityType = filter.EntityType.Value;
                q = q.Where(l => l.EntityType == entityType);
            }

            if (filter.EntityId.HasValue)
            {
                var entityId = filter.EntityId.Value;
                q = q.Where(l => l.EntityId == entityId);
            }

            if (filter.UserId.HasValue)
            {
                var userId = filter.UserId.Value;
                q = q.Where(l => l.UserId == userId);
            }

            if (filter.Operation.HasValue)
            {
                var operation = filter.Operation.Value;
                q = q.Where(l => l.Operation == operation);
            }

            if (filter.From.HasValue)
            {
                var from = filter.From.Value.ToDateTime(TimeOnly.MinValue);
                q = q.Where(l => l.OccurredAt >= from);
            }

            if (filter.To.HasValue)
            {
                var toExclusive = filter.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                q = q.Where(l => l.OccurredAt < toExclusive);
            }

            if (!string.IsNullOrWhiteSpace(paging.SearchTerm))
            {
                var term = paging.SearchTerm.Trim();
                q = q.Where(l => l.UserName != null && l.UserName.Contains(term));
            }

            var total = await q.CountAsync(ct);

            // Id grows with time and is the clustered key: cheaper than sorting by OccurredAt
            var items = await q
                .OrderByDescending(l => l.Id)
                .Skip((paging.Page - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .Select(l => new AuditLogListItem
                {
                    Id = l.Id,
                    EntityType = l.EntityType,
                    EntityId = l.EntityId,
                    Operation = l.Operation,
                    UserId = l.UserId,
                    UserName = l.UserName,
                    OccurredAt = l.OccurredAt
                })
                .ToListAsync(ct);

            return new PagedResponse<AuditLogListItem>
            {
                Items = items,
                TotalCount = total,
                Page = paging.Page,
                PageSize = paging.PageSize
            };
        }

        public async Task<AuditLogResponse?> GetByIdAsync(long id, CancellationToken ct = default)
            => await _repo.Query()
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new AuditLogResponse
                {
                    Id = l.Id,
                    EntityType = l.EntityType,
                    EntityId = l.EntityId,
                    Operation = l.Operation,
                    UserId = l.UserId,
                    UserName = l.UserName,
                    DataBefore = l.DataBefore,
                    DataAfter = l.DataAfter,
                    OccurredAt = l.OccurredAt
                })
                .FirstOrDefaultAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public Task<AuditSettingsOptions> GetSettingsAsync(CancellationToken ct = default)
    => _settings.GetAsync(ct);

        public async Task UpdateSettingsAsync(AuditSettingsOptions request, CancellationToken ct = default)
        {
            if (!_currentUser.IsAuthenticated)
                throw new InvalidOperationException("Changing audit settings requires an authenticated user.");

            if (request.RetentionDays < AuditSettingsOptions.MinRetentionDays)
                throw new InvalidOperationException(
                    $"Retention cannot be less than {AuditSettingsOptions.MinRetentionDays} days.");

            var current = await _settings.GetAsync(ct);
            if (current.RetentionDays == request.RetentionDays) return;

            // Record first: a failed file write leaves a record of the attempt, never a silent change
            await _repo.AddAsync(Domain.Entities.UsersLog.AuditLog.Create(
                EntityType.AuditLog,
                Guid.Empty,
                AuditOperation.Update,
                _currentUser.UserId,
                _currentUser.UserName,
                JsonSerializer.Serialize(new { current.RetentionDays }),
                JsonSerializer.Serialize(new { request.RetentionDays })), ct);

            await _settings.SaveAsync(new AuditSettingsOptions { RetentionDays = request.RetentionDays }, ct);

            _logger.LogWarning(
                "Audit retention changed by {UserName}: {Old} -> {New} days",
                _currentUser.UserName, current.RetentionDays, request.RetentionDays);
        }


        public async Task<PurgeAuditLogsResponse> PurgeAsync(DateOnly? before, CancellationToken ct = default)
        {
            // [Authorize] already guarantees this; the purge record must never be anonymous
            if (!_currentUser.IsAuthenticated)
                throw new InvalidOperationException("Purging audit logs requires an authenticated user.");

            var configured = await GetRetentionDaysAsync(ct);
            if (configured < AuditSettingsOptions.MinRetentionDays)
                _logger.LogWarning(
                    "Audit retention setting {Configured} is below the minimum; using {Min} days",
                    configured, AuditSettingsOptions.MinRetentionDays);

            var retentionDays = Math.Max(configured, AuditSettingsOptions.MinRetentionDays);

            var maxCutoff = DateTime.UtcNow.Date.AddDays(-retentionDays);

            var cutoff = before.HasValue
                ? before.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                : maxCutoff;

            if (cutoff > maxCutoff)
                throw new InvalidOperationException(
                    $"Only audit logs older than {retentionDays} days can be deleted. " +
                    $"Latest allowed date: {DateOnly.FromDateTime(maxCutoff):yyyy-MM-dd}.");

            var deleted = await _repo.PurgeAsync(cutoff, count => Domain.Entities.UsersLog.AuditLog.Create(
                EntityType.AuditLog,
                Guid.Empty,
                AuditOperation.Purge,
                _currentUser.UserId,
                _currentUser.UserName,
                JsonSerializer.Serialize(new
                {
                    Cutoff = cutoff,
                    RetentionDays = retentionDays,
                    DeletedCount = count
                }),
                null), ct);

            _logger.LogWarning(
                "Audit logs purged by {UserName}: {DeletedCount} records older than {Cutoff:yyyy-MM-dd} (retention {RetentionDays} days)",
                _currentUser.UserName, deleted, cutoff, retentionDays);

            return new PurgeAuditLogsResponse
            {
                DeletedCount = deleted,
                Cutoff = cutoff,
                RetentionDays = retentionDays
            };
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        // Temporary: wired to the admin settings file in the next step
        private async Task<int> GetRetentionDaysAsync(CancellationToken ct)
     => (await _settings.GetAsync(ct)).RetentionDays;
    }
}