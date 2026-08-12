

using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using System.Net.Http.Json;

namespace Marilog.Client.Services.EmailNotification
{
    public sealed class JsonNotificationSenderEmailSettingsStore : INotificationSenderEmailSettingsStore
    {
        private const string Base = "api/NotificationEmailSettings";

        private readonly HttpClient _http;

        public JsonNotificationSenderEmailSettingsStore(HttpClient http)
        {
            _http = http;
        }

        public async Task<NotificationSenderEmailSettingsOptions> GetAsync(CancellationToken cancellationToken = default)
        {
            var options =
                await _http.GetFromJsonAsync<NotificationSenderEmailSettingsOptions>(
                    Base,
                    cancellationToken);

            return options ?? throw new InvalidOperationException("Notification email settings response was empty.");
        }

        public async Task UpdateAsync(NotificationSenderEmailSettingsOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            var response = await _http.PutAsJsonAsync(
                Base,
                options,
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }
}
