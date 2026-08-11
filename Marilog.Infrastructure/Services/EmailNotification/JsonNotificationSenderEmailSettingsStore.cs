using Marilog.Application.Interfaces.Encryption;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Marilog.Contracts.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace Marilog.Infrastructure.Services.EmailNotification
{
    public sealed class JsonNotificationSenderEmailSettingsStore : INotificationSenderEmailSettingsStore
    {
        private const string DefaultFileName = "notification-email-settings.json";

        private readonly ILogger<JsonNotificationSenderEmailSettingsStore> _logger;
        private readonly ISecretEncryptionService _encryptionService;
        private readonly string _filePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly SemaphoreSlim _lock = new(1, 1);

        public JsonNotificationSenderEmailSettingsStore(
            ILogger<JsonNotificationSenderEmailSettingsStore> logger,
            IConfiguration configuration,
            ISecretEncryptionService encryptionService)
        {
            _logger = logger;
            _encryptionService = encryptionService;

            var configuredPath =
                configuration["DailyNotification:EmailSettingsFilePath"];

            _filePath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(
                    AppContext.BaseDirectory,
                    "Config",
                    DefaultFileName)
                : Path.GetFullPath(configuredPath);
        }

        public async Task<NotificationSenderEmailSettingsOptions> GetAsync(
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

        public async Task UpdateAsync(NotificationSenderEmailSettingsOptions options, CancellationToken cancellationToken = default)
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

        private async Task<NotificationSenderEmailSettingsOptions> LoadAsync(CancellationToken cancellationToken)
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

                var encryptedSettings = await JsonSerializer.DeserializeAsync<EncryptedNotificationEmailSettings>(stream, _jsonOptions, cancellationToken);

                if (encryptedSettings is null || string.IsNullOrWhiteSpace(encryptedSettings.Data))
                {
                    return new NotificationSenderEmailSettingsOptions();
                }

                var plainText = _encryptionService.Decrypt(encryptedSettings.Data);

                var options = JsonSerializer.Deserialize<NotificationSenderEmailSettingsOptions>( plainText, _jsonOptions);

                return options ?? new NotificationSenderEmailSettingsOptions();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid notification email settings configuration file: {FilePath}", _filePath);

                throw new InvalidOperationException("The notification email settings configuration file contains invalid JSON.", ex);
            }
            catch (Exception ex) when (ex is CryptographicException || ex is FormatException)
            {
                _logger.LogError(ex, "Failed to decrypt notification email settings configuration file: {FilePath}", _filePath);

                throw new InvalidOperationException("The notification email settings configuration file could not be decrypted.", ex);
            }
        }

        private async Task SaveAsync(NotificationSenderEmailSettingsOptions options, CancellationToken cancellationToken)
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException($"Invalid notification email settings file path: {_filePath}");
            }

            Directory.CreateDirectory(directory);

            var plainText = JsonSerializer.Serialize(options, _jsonOptions);

            var encryptedData = _encryptionService.Encrypt(plainText);

            var encryptedSettings =
                new EncryptedNotificationEmailSettings
                {
                    Data = encryptedData
                };

            var temporaryFilePath = $"{_filePath}.{Guid.NewGuid():N}.tmp";

            try
            {
                await using (var stream = new FileStream(temporaryFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await JsonSerializer.SerializeAsync(stream, encryptedSettings, _jsonOptions, cancellationToken);

                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(temporaryFilePath, _filePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryFilePath))
                {
                    File.Delete(temporaryFilePath);
                }
            }
        }

        private async Task EnsureFileExistsAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(_filePath))
                return;

            var directory = Path.GetDirectoryName(_filePath);

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException(
                    $"Invalid notification email settings file path: {_filePath}");
            }

            Directory.CreateDirectory(directory);

            var defaultOptions = new NotificationSenderEmailSettingsOptions();

            var plainText = JsonSerializer.Serialize(defaultOptions, _jsonOptions);

            var encryptedData = _encryptionService.Encrypt(plainText);

            var encryptedSettings = new EncryptedNotificationEmailSettings
            {
                Data = encryptedData
            };

            await using var stream = new FileStream(_filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true);

            await JsonSerializer.SerializeAsync(stream, encryptedSettings, _jsonOptions, cancellationToken);
        }

        private sealed class EncryptedNotificationEmailSettings
        {
            public string Data { get; set; } = string.Empty;
        }
    }
}
