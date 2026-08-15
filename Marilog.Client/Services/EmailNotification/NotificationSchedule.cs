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

        public async Task<NotificationScheduleOptions> GetAsync(CancellationToken cancellationToken = default)
        {
            var response =await _httpClient.GetAsync(Endpoint, cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NotificationScheduleOptions>(cancellationToken: cancellationToken);

            return result ?? throw new InvalidOperationException("Notification schedule response was empty.");
        }

        public async Task SaveAsync(NotificationScheduleOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            var response = await _httpClient.PutAsJsonAsync(Endpoint, options, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task<DateTimeOffset?> GetNextExecutionAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"{Endpoint}/next-execution?now={Uri.EscapeDataString(now.ToString("O"))}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DateTimeOffset>(
                cancellationToken: cancellationToken);
        }
    }
}
