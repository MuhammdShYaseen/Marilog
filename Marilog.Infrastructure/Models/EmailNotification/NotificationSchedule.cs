using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Marilog.Infrastructure.Models.EmailNotification
{
    public class NotificationSchedule: INotificationSchedule
    {
        private const string DefaultFileName = "notification-schedule.json";

        private readonly string _filePath;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public NotificationSchedule(IConfiguration configuration)
        {
            var configuredPath = configuration["DailyNotification:ScheduleFilePath"];

            _filePath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(
                    AppContext.BaseDirectory,
                    DefaultFileName)
                : Path.GetFullPath(configuredPath);
        }

        public async Task<NotificationScheduleOptions> GetAsync(CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_filePath))
            {
                var defaultOptions = new NotificationScheduleOptions();

                await SaveAsync(defaultOptions, cancellationToken);

                return defaultOptions;
            }

            await using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

            var options = await JsonSerializer.DeserializeAsync<NotificationScheduleOptions>(stream, JsonOptions, cancellationToken);

            return options ?? new NotificationScheduleOptions();
        }

        public async Task SaveAsync(NotificationScheduleOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            Validate(options);

            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            await using var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);

            await JsonSerializer.SerializeAsync(stream, options, JsonOptions, cancellationToken);
        }

        public async Task<DateTimeOffset?> GetNextExecutionAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var options = await GetAsync(cancellationToken);

            if (!options.Enabled)
                return null;

            var timeZone = GetTimeZone(options.TimeZoneId);

            var localNow =TimeZoneInfo.ConvertTime(now, timeZone);

            var nextExecution = new DateTime(
                localNow.Year,
                localNow.Month,
                localNow.Day,
                options.ExecutionTime.Hour,
                options.ExecutionTime.Minute,
                options.ExecutionTime.Second,
                DateTimeKind.Unspecified);

            if (nextExecution <= localNow.DateTime)
            {
                nextExecution = nextExecution.AddDays(1);
            }

            var utcExecution = TimeZoneInfo.ConvertTimeToUtc(nextExecution, timeZone);

            return new DateTimeOffset(utcExecution, TimeSpan.Zero);
        }

        private static void Validate(NotificationScheduleOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.TimeZoneId))
            {
                throw new ArgumentException("Notification time zone is required.", nameof(options));
            }

            _ = GetTimeZone(options.TimeZoneId);
        }

        private static TimeZoneInfo GetTimeZone(string timeZoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new InvalidOperationException(
                    $"Notification time zone '{timeZoneId}' was not found.");
            }
            catch (InvalidTimeZoneException)
            {
                throw new InvalidOperationException(
                    $"Notification time zone '{timeZoneId}' is invalid.");
            }
        }
    }
}
