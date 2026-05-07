using Marilog.Contracts.DTOs.Frontend.AppTheme;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.Interfaces.FrontendServices
{
    public interface IAppThemeService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<AppThemeResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<AppThemeResponse?> GetDefaultThemeAsync(CancellationToken ct = default);
        Task<IReadOnlyList<AppThemeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<AppThemeResponse>> GetActiveAsync(CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<AppThemeResponse> CreateAsync(CreateAppThemeRequest request, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateAppThemeRequest request, CancellationToken ct = default);
        Task SetAsDefaultAsync(int id, CancellationToken ct = default);
        Task ActivateAsync(int id, CancellationToken ct = default);
        Task DeactivateAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
