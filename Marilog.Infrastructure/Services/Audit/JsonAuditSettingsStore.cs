using Marilog.Contracts.DTOs.Requests.AuditDTOs;
using Marilog.Contracts.Interfaces.Services.AuditLog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Marilog.Infrastructure.Services.Audit
{
    public sealed class JsonAuditSettingsStore : IAuditSettingsStore
    {
        private const string DefaultFileName = "audit-settings.json";

        private readonly ILogger<JsonAuditSettingsStore> _logger;
        private readonly string _filePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly SemaphoreSlim _lock = new(1, 1);

        public JsonAuditSettingsStore(ILogger<JsonAuditSettingsStore> logger, IConfiguration configuration)
        {
            _logger = logger;

            var configuredPath = configuration["AuditSettings:FilePath"];

            _filePath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(AppContext.BaseDirectory, "Config", DefaultFileName)
                : Path.GetFullPath(configuredPath);
        }

        public async Task<AuditSettingsOptions> GetAsync(CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);

            try
            {
                return await LoadAsync(ct);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveAsync(AuditSettingsOptions settings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(settings);

            await _lock.WaitAsync(ct);

            try
            {
                await WriteAsync(settings, ct);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<AuditSettingsOptions> LoadAsync(CancellationToken ct)
        {
            if (!File.Exists(_filePath))
            {
                var defaults = new AuditSettingsOptions();
                await WriteAsync(defaults, ct);
                return defaults;
            }

            try
            {
                await using var stream = new FileStream(
                    _filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);

                return await JsonSerializer.DeserializeAsync<AuditSettingsOptions>(stream, _jsonOptions, ct)
                       ?? new AuditSettingsOptions();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid audit settings file: {FilePath}", _filePath);

                throw new InvalidOperationException("The audit settings file contains invalid JSON.", ex);
            }
        }

        private async Task WriteAsync(AuditSettingsOptions settings, CancellationToken ct)
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException($"Invalid audit settings file path: {_filePath}");

            Directory.CreateDirectory(directory);

            var temporaryFilePath = $"{_filePath}.{Guid.NewGuid():N}.tmp";

            try
            {
                await using (var stream = new FileStream(
                    temporaryFilePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true))
                {
                    await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions, ct);
                    await stream.FlushAsync(ct);
                }

                File.Move(temporaryFilePath, _filePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryFilePath))
                    File.Delete(temporaryFilePath);
            }
        }
    }
}