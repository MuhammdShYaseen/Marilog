using Marilog.Contracts.DTOs.Requests.AuditDTOs;

namespace Marilog.Contracts.Interfaces.Services.AuditLog
{
    public interface IAuditSettingsStore
    {
        Task<AuditSettingsOptions> GetAsync(CancellationToken ct = default);
        Task SaveAsync(AuditSettingsOptions settings, CancellationToken ct = default);
    }
}
