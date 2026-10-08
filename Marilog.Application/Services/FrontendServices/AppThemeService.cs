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
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<AppThemeResponse?> GetDefaultThemeAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .Where(t => t.IsDefault)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<AppThemeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<AppThemeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .Where(t => t.IsActive)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<AppThemeResponse> CreateAsync(CreateAppThemeRequest request, CancellationToken ct = default)
        {
            if (await _repo.Query().AnyAsync(t => t.ThemeKey == request.ThemeKey, ct))
                throw new InvalidOperationException($"Theme key '{request.ThemeKey}' already exists");

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

        public async Task<IReadOnlyList<AppThemeResponse>> CreateRangeAsync(IReadOnlyList<CreateAppThemeRequest> requests, CancellationToken ct = default)
        {
            if (requests is null || requests.Count == 0)
                throw new InvalidOperationException("No themes provided");

            var duplicateInRequest = requests
                .GroupBy(r => r.ThemeKey)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateInRequest.Count > 0)
                throw new InvalidOperationException(
                    $"Duplicate theme keys in request: {string.Join(", ", duplicateInRequest)}");

            var keys = requests.Select(r => r.ThemeKey).ToList();

            var existingKeys = await _repo.Query()
                .Where(t => keys.Contains(t.ThemeKey))
                .Select(t => t.ThemeKey)
                .ToListAsync(ct);

            if (existingKeys.Count > 0)
                throw new InvalidOperationException(
                    $"Theme keys already exist: {string.Join(", ", existingKeys)}");

            var themes = requests.Select(request => AppTheme.Create(
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
                request.IsDarkMode)).ToList();

            foreach (var theme in themes)
                await _repo.AddAsync(theme, ct);

            await _repo.SaveChangesAsync(ct);

            return themes.Select(MapToResponse).ToList();
        }

        public async Task UpdateAsync(int id, UpdateAppThemeRequest request, CancellationToken ct = default)
        {
            var theme = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Theme not found");

            if (request.IsDefault == true && !theme.IsActive)
                throw new InvalidOperationException("Cannot set an inactive theme as default");

            if (request.IsDefault == false && theme.IsDefault)
                throw new InvalidOperationException("Cannot unset the default theme directly. Set another theme as default instead.");

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

            if (!theme.IsActive)
                throw new InvalidOperationException("Cannot set an inactive theme as default");

            if (theme.IsDefault)
                return;

            await UnsetAllDefaultsAsync(ct);
            await _repo.SaveChangesAsync(ct);

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

            if (theme.IsDefault)
                throw new InvalidOperationException("Cannot deactivate the default theme. Set another theme as default first.");

            theme.Deactivate();
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var theme = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Theme not found");

            if (theme.IsDefault)
                throw new InvalidOperationException("Cannot delete the default theme. Set another theme as default first.");

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

        private static readonly Expression<Func<AppTheme, AppThemeResponse>> ToResponse = t => new AppThemeResponse
        {
            Id = t.Id,
            ThemeName = t.ThemeName,
            ThemeKey = t.ThemeKey,
            IsDefault = t.IsDefault,
            IsActive = t.IsActive,
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

        private static readonly Func<AppTheme, AppThemeResponse> MapToResponse = ToResponse.Compile();
    }
}
