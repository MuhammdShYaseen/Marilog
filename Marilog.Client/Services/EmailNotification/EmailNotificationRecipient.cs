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

        public async Task<IReadOnlyList<string>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync(BaseUrl, ct);

            await response.EnsureSuccessWithMessageAsync(ct);

            return await response.Content.ReadFromJsonAsync<IReadOnlyList<string>>(ct) ?? [];
        }

        public async Task AddAsync(string email, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, email, ct);

            await response.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task AddRangeAsync(IEnumerable<string> emails, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/bulk", emails, ct);

            await response.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task UpdateAsync(string currentEmail, string newEmail, CancellationToken ct = default)
        {
            var request = new UpdateNotificationRecipientRequest
            { 
                CurrentEmail = currentEmail,
                NewEmail = newEmail
            };

            var response = await _httpClient.PutAsJsonAsync(BaseUrl, request, ct);

            await response.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task RemoveAsync(string email, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}?email={Uri.EscapeDataString(email)}";

            var response = await _httpClient.DeleteAsync(url, ct);

            await response.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task RemoveRangeAsync(IEnumerable<string> emails, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/bulk")
            {
                Content = JsonContent.Create(emails)
            };

            using (request)
            {
                var response = await _httpClient.SendAsync(request, ct);

                await response.EnsureSuccessWithMessageAsync(ct);
            }
        }
    }
}
