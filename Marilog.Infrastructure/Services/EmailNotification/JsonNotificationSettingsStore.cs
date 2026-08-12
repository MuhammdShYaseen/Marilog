using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Marilog.Infrastructure.Services.EmailNotification
{
    public sealed class JsonNotificationSettingsStore : INotificationSettingsStore
    {
        private const string DefaultFileName = "notification-settings.json";

        private readonly ILogger<JsonNotificationSettingsStore> _logger;
        private readonly string _filePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly SemaphoreSlim _lock = new(1, 1);

        public JsonNotificationSettingsStore(
            ILogger<JsonNotificationSettingsStore> logger,
            IConfiguration configuration)
        {
            _logger = logger;

            var configuredPath = configuration["DailyNotification:SettingsFilePath"];

            _filePath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(
                    AppContext.BaseDirectory,
                    "Config",
                    DefaultFileName)
                : Path.GetFullPath(configuredPath);
        }

        public async Task<NotificationSettingsOptions> GetAsync(
            CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                return await LoadAsync(cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateAsync(
            NotificationSettingsOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            await _lock.WaitAsync(cancellationToken);

            try
            {
                await SaveAsync(options, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<NotificationSettingsOptions> LoadAsync(
            CancellationToken cancellationToken)
        {
            await EnsureFileExistsAsync(cancellationToken);

            try
            {
                await using var stream = new FileStream(
                    _filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);

                var options = await JsonSerializer.DeserializeAsync<NotificationSettingsOptions>(
                    stream,
                    _jsonOptions,
                    cancellationToken);

                return options ?? new NotificationSettingsOptions();
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid notification settings configuration file: {FilePath}",
                    _filePath);

                throw new InvalidOperationException(
                    "The notification settings configuration file contains invalid JSON.",
                    ex);
            }
        }

        private async Task SaveAsync(
            NotificationSettingsOptions options,
            CancellationToken cancellationToken)
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException(
                    $"Invalid notification settings file path: {_filePath}");

            Directory.CreateDirectory(directory);

            var temporaryFilePath =
                $"{_filePath}.{Guid.NewGuid():N}.tmp";

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
                    await JsonSerializer.SerializeAsync(
                        stream,
                        options,
                        _jsonOptions,
                        cancellationToken);

                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(
                    temporaryFilePath,
                    _filePath,
                    overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryFilePath))
                    File.Delete(temporaryFilePath);
            }
        }

        private async Task EnsureFileExistsAsync(
            CancellationToken cancellationToken)
        {
            if (File.Exists(_filePath))
                return;

            var directory = Path.GetDirectoryName(_filePath);

            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException(
                    $"Invalid notification settings file path: {_filePath}");

            Directory.CreateDirectory(directory);

            var options = new NotificationSettingsOptions();

            await using var stream = new FileStream(
                _filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            await JsonSerializer.SerializeAsync(
                stream,
                options,
                _jsonOptions,
                cancellationToken);
        }
    }
}
