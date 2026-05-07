using Marilog.Contracts.DTOs.Frontend.AppTheme;
using Marilog.Contracts.Interfaces.FrontendServices;
using Marilog.Domain.Entities.Frontend;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.FrontendServices
{
    public class AppThemeService : IAppThemeService
    {
        private readonly IRepository<AppTheme> _repo;

        public AppThemeService(IRepository<AppTheme> repo)
        {
            _repo = repo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<AppThemeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .Where(t => t.Id == id)
                .Select(ToResponse())
                .FirstOrDefaultAsync(ct);
        }

        public async Task<AppThemeResponse?> GetDefaultThemeAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .Where(t => t.IsDefault)
                .Select(ToResponse())
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<AppThemeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .Select(ToResponse())
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<AppThemeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .Where(t => t.IsActive)
                .Select(ToResponse())
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<AppThemeResponse> CreateAsync(CreateAppThemeRequest request, CancellationToken ct = default)
        {
            if (request.IsDefault)
                await UnsetAllDefaultsAsync(ct);

            var theme = AppTheme.Create(
                request.ThemeName,
                request.ThemeKey,
                request.IsDefault,
                request.PrimaryColor,
                request.SecondaryColor,
                request.AppBarColor,
                request.BackgroundColor,
                request.SurfaceColor,
                request.ErrorColor,
                request.SuccessColor,
                request.WarningColor,
                request.FontFamily,
                request.BaseFontSize,
                request.IsDarkMode);

            await _repo.AddAsync(theme, ct);
            await _repo.SaveChangesAsync(ct);

            return MapToResponse(theme);
        }

        public async Task UpdateAsync(int id, UpdateAppThemeRequest request, CancellationToken ct = default)
        {
            var theme = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Theme not found");

            if (request.IsDefault == true)
                await UnsetAllDefaultsAsync(ct);

            theme.Update(
                request.ThemeName,
                request.PrimaryColor,
                request.SecondaryColor,
                request.AppBarColor,
                request.BackgroundColor,
                request.SurfaceColor,
                request.ErrorColor,
                request.SuccessColor,
                request.WarningColor,
                request.FontFamily,
                request.BaseFontSize,
                request.IsDarkMode,
                request.IsDefault);

            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetAsDefaultAsync(int id, CancellationToken ct = default)
        {
            var theme = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Theme not found");

            await UnsetAllDefaultsAsync(ct);

            theme.SetAsDefault();
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var theme = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Theme not found");

            theme.Activate();
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var theme = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Theme not found");

            theme.Deactivate();
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var theme = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Theme not found");

            _repo.HardDelete(theme);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private async Task UnsetAllDefaultsAsync(CancellationToken ct)
        {
            var defaults = await _repo.Query()
                .Where(t => t.IsDefault)
                .ToListAsync(ct);

            foreach (var t in defaults)
                t.UnsetDefault();
        }

        private static Expression<Func<AppTheme, AppThemeResponse>> ToResponse() => t => new AppThemeResponse
        {
            Id = t.Id,
            ThemeName = t.ThemeName,
            ThemeKey = t.ThemeKey,
            IsDefault = t.IsDefault,
            PrimaryColor = t.PrimaryColor,
            SecondaryColor = t.SecondaryColor,
            AppBarColor = t.AppBarColor,
            BackgroundColor = t.BackgroundColor,
            SurfaceColor = t.SurfaceColor,
            ErrorColor = t.ErrorColor,
            SuccessColor = t.SuccessColor,
            WarningColor = t.WarningColor,
            FontFamily = t.FontFamily,
            BaseFontSize = t.BaseFontSize,
            IsDarkMode = t.IsDarkMode
        };

        private static AppThemeResponse MapToResponse(AppTheme t) => new()
        {
            Id = t.Id,
            ThemeName = t.ThemeName,
            ThemeKey = t.ThemeKey,
            IsDefault = t.IsDefault,
            PrimaryColor = t.PrimaryColor,
            SecondaryColor = t.SecondaryColor,
            AppBarColor = t.AppBarColor,
            BackgroundColor = t.BackgroundColor,
            SurfaceColor = t.SurfaceColor,
            ErrorColor = t.ErrorColor,
            SuccessColor = t.SuccessColor,
            WarningColor = t.WarningColor,
            FontFamily = t.FontFamily,
            BaseFontSize = t.BaseFontSize,
            IsDarkMode = t.IsDarkMode
        };
    }
}
