using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Marilog.Contracts.Options;
using System.Net.Http.Json;

namespace Marilog.Client.Services.EmailNotification
{
    public sealed class JsonNotificationSettingsStore : INotificationSettingsStore
    {
        private const string Base = "api/NotificationSettings";

        private readonly HttpClient _http;

        public JsonNotificationSettingsStore(HttpClient http)
        {
            _http = http;
        }

        public async Task<NotificationSettingsOptions> GetAsync(CancellationToken cancellationToken = default)
        {
            var options = await _http.GetFromJsonAsync<NotificationSettingsOptions>(Base, cancellationToken);

            return options ?? throw new InvalidOperationException("Notification settings response was empty.");
        }

        public async Task UpdateAsync(NotificationSettingsOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            var response = await _http.PutAsJsonAsync(Base, options, cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }
}
