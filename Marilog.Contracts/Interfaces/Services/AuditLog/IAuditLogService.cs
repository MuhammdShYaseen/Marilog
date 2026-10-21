using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.AuditDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.AuditLog
{
    public interface IAuditLogService
    {
        Task<PagedResponse<AuditLogListItem>> GetPagedAsync(PagedRequest paging, AuditLogFilter filter, CancellationToken ct = default);
        Task<AuditLogResponse?> GetByIdAsync(long id, CancellationToken ct = default);

        /// <summary>
        /// Deletes logs older than <paramref name="before"/> (UTC, exclusive).
        /// Null means everything older than the retention period.
        /// Purge records are never deleted.
        /// </summary>
        Task<PurgeAuditLogsResponse> PurgeAsync(DateOnly? before, CancellationToken ct = default);

        Task<AuditSettingsOptions> GetSettingsAsync(CancellationToken ct = default);
        Task UpdateSettingsAsync(AuditSettingsOptions request, CancellationToken ct = default);
    }
}
