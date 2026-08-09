using Marilog.Application.Interfaces.EmailNotificationConfig;
using Marilog.Contracts.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Text.Json;

namespace Marilog.Infrastructure.Services.EmailNotification
{
    public sealed class JsonNotificationRecipientStore : INotificationRecipientStore
    {
        private const string DefaultFileName = "notification-recipients.json";

        private readonly ILogger<JsonNotificationRecipientStore> _logger;
        private readonly string _filePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly SemaphoreSlim _lock = new(1, 1);

        public JsonNotificationRecipientStore(ILogger<JsonNotificationRecipientStore> logger, IConfiguration configuration)
        {
            _logger = logger;

            var configuredPath = configuration["DailyNotification:RecipientsFilePath"];

            _filePath = string.IsNullOrWhiteSpace(configuredPath) ? Path.Combine(AppContext.BaseDirectory, "Config", DefaultFileName) : Path.GetFullPath(configuredPath);
        }

        public async Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var options = await LoadAsync(cancellationToken);

                return options.Recipients.ToArray();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task AddAsync(string email, CancellationToken cancellationToken = default)
        {
            ValidateEmail(email);

            await _lock.WaitAsync(cancellationToken);

            try
            {
                var options = await LoadAsync(cancellationToken);

                var normalizedEmail = NormalizeEmail(email);

                if (options.Recipients.Any(x => string.Equals(x, normalizedEmail, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
                options.Recipients.Add(normalizedEmail);
                NormalizeRecipients(options);
                await SaveAsync(options, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task AddRangeAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(emails);
            var normalizedEmails = emails
                .Select(NormalizeEmail)
                .ToList();

            foreach (var email in normalizedEmails)
                ValidateEmail(email);

            await _lock.WaitAsync(cancellationToken);

            try
            {
                var options = await LoadAsync(cancellationToken);

                options.Recipients.AddRange(normalizedEmails);

                NormalizeRecipients(options);

                await SaveAsync(options, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateAsync(string currentEmail, string newEmail, CancellationToken cancellationToken = default)
        {
            ValidateEmail(currentEmail);
            ValidateEmail(newEmail);
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var options = await LoadAsync(cancellationToken);

                var currentIndex = options.Recipients.FindIndex(x => string.Equals(x, currentEmail, StringComparison.OrdinalIgnoreCase));

                if (currentIndex < 0)
                    throw new KeyNotFoundException($"Notification recipient '{currentEmail}' was not found.");

                var duplicateExists = options.Recipients
                    .Where((_, index) => index != currentIndex)
                    .Any(x => string.Equals(x, newEmail,StringComparison.OrdinalIgnoreCase));

                if (duplicateExists)
                    throw new InvalidOperationException($"Notification recipient '{newEmail}' already exists.");

                options.Recipients[currentIndex] = NormalizeEmail(newEmail);

                NormalizeRecipients(options);

                await SaveAsync(options, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveAsync(string email, CancellationToken cancellationToken = default)
        {
            ValidateEmail(email);

            await _lock.WaitAsync(cancellationToken);

            try
            {
                var options = await LoadAsync(cancellationToken);

                var removed = options.Recipients.RemoveAll(x => string.Equals(x, email, StringComparison.OrdinalIgnoreCase));

                if (removed == 0)
                    return;

                await SaveAsync(options, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveRangeAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(emails);

            var emailSet = emails
                .Select(NormalizeEmail)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var email in emailSet)
                ValidateEmail(email);

            await _lock.WaitAsync(cancellationToken);

            try
            {
                var options = await LoadAsync(cancellationToken);

                options.Recipients.RemoveAll(emailSet.Contains);

                await SaveAsync(options, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<NotificationRecipientsOptions> LoadAsync(CancellationToken cancellationToken)
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

                var options = await JsonSerializer.DeserializeAsync<NotificationRecipientsOptions>(stream, _jsonOptions, cancellationToken);

                if (options is null)
                    return new NotificationRecipientsOptions();

                NormalizeRecipients(options);

                return options;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid notification recipients configuration file: {FilePath}", _filePath);

                throw new InvalidOperationException("The notification recipients configuration file contains invalid JSON.", ex);
            }
        }

        private async Task SaveAsync(NotificationRecipientsOptions options, CancellationToken cancellationToken)
        {
            NormalizeRecipients(options);

            var directory = Path.GetDirectoryName(_filePath);

            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException($"Invalid notification recipients file path: {_filePath}");

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
                    await JsonSerializer.SerializeAsync(stream, options, _jsonOptions, cancellationToken);

                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(temporaryFilePath, _filePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryFilePath))
                    File.Delete(temporaryFilePath);
            }
        }

        private async Task EnsureFileExistsAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(_filePath))
                return;

            var directory = Path.GetDirectoryName(_filePath);

            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException($"Invalid notification recipients file path: {_filePath}");

            Directory.CreateDirectory(directory);

            var options = new NotificationRecipientsOptions();

            await using var stream = new FileStream(
                _filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            await JsonSerializer.SerializeAsync(stream, options, _jsonOptions, cancellationToken);
        }

        private static void NormalizeRecipients(NotificationRecipientsOptions options)
        {
            options.Recipients = options.Recipients
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeEmail)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizeEmail(string email) =>
            email.Trim();

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email address is required.", nameof(email));

            try
            {
                var address = new MailAddress(email);

                if (!string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException();
                }
            }
            catch
            {
                throw new ArgumentException($"Invalid email address: '{email}'.", nameof(email));
            }
        }
    }
}
