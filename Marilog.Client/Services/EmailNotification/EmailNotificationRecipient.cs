using Marilog.Contracts.DTOs.Requests.EmailNotifiConfig;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using System.Net.Http.Json;

namespace Marilog.Client.Services.EmailNotification
{
    public sealed class EmailNotificationRecipient: INotificationRecipientStore
    {
        private const string BaseUrl = "api/notification-recipients";

        private readonly HttpClient _httpClient;

        public EmailNotificationRecipient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(BaseUrl, cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IReadOnlyList<string>>(cancellationToken) ?? [];
        }

        public async Task AddAsync(string email, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, email, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task AddRangeAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/bulk", emails, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(string currentEmail, string newEmail, CancellationToken cancellationToken = default)
        {
            var request = new UpdateNotificationRecipientRequest
            { 
                CurrentEmail = currentEmail,
                NewEmail = newEmail
            };

            var response = await _httpClient.PutAsJsonAsync(BaseUrl, request, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveAsync(string email, CancellationToken cancellationToken = default)
        {
            var url = $"{BaseUrl}?email={Uri.EscapeDataString(email)}";

            var response = await _httpClient.DeleteAsync(url, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveRangeAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/bulk")
            {
                Content = JsonContent.Create(emails)
            };

            using (request)
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);

                response.EnsureSuccessStatusCode();
            }
        }
    }
}
