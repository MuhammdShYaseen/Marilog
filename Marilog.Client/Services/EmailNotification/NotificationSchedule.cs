using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using System.Net.Http.Json;

namespace Marilog.Client.Services.EmailNotification
{
    public class NotificationSchedule : INotificationSchedule
    {
        private readonly HttpClient _httpClient;

        private const string Endpoint = "api/email-notifications/schedule";

        public NotificationSchedule(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NotificationScheduleOptions> GetAsync(CancellationToken ct = default)
        {
            var response =await _httpClient.GetAsync(Endpoint, ct);

            await response.EnsureSuccessWithMessageAsync(ct);

            var result = await response.Content.ReadFromJsonAsync<NotificationScheduleOptions>(cancellationToken: ct);

            return result ?? throw new InvalidOperationException("Notification schedule response was empty.");
        }

        public async Task SaveAsync(NotificationScheduleOptions options, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            var response = await _httpClient.PutAsJsonAsync(Endpoint, options, ct);

            await response.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task<DateTimeOffset?> GetNextExecutionAsync(DateTimeOffset now, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync($"{Endpoint}/next-execution?now={Uri.EscapeDataString(now.ToString("O"))}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return null;

            await response.EnsureSuccessWithMessageAsync(ct);

            return await response.Content.ReadFromJsonAsync<DateTimeOffset>(
                cancellationToken: ct);
        }
    }
}
