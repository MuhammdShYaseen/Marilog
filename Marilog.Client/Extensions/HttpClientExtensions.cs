using Marilog.Client.ErrorUniform;
using Marilog.Contracts.Common;
using System.Net;
using System.Net.Http.Json;

namespace Marilog.Client.Extensions
{
    public static class HttpClientExtensions
    {
        // ─────────────────────────────────────────────
        // Core executor (handles transport errors only)
        // ─────────────────────────────────────────────
        private static async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> action, CancellationToken ct)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(null, ex.Message, ex.StatusCode?.ToString() ?? "HttpRequestException");
            }
        }

        // ─────────────────────────────────────────────
        // Core response handler (handles API contract)
        // ─────────────────────────────────────────────
        private static async Task<T?> HandleAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return default;

                var success = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct);

                return success is null ? default : success.Data;
            }

            ApiErrorResponse? error = null;

            try
            {
                error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken: ct);
            }
            catch
            {
                // ignore malformed error payload
            }

            throw new ApiException(error, response.StatusCode.ToString(), error?.Message ?? response.ReasonPhrase ?? "Request failed");
        }

        // ─────────────────────────────────────────────
        // GET (single)
        // ─────────────────────────────────────────────
        public static async Task<T?> GetApiAsync<T>(this HttpClient http, string url, CancellationToken ct = default)
        {
            var response = await ExecuteAsync(() => http.GetAsync(url, ct), ct);
            return await HandleAsync<T>(response, ct);
        }

        // ─────────────────────────────────────────────
        // GET (list)
        // ─────────────────────────────────────────────
        public static async Task<IReadOnlyList<T>> GetApiListAsync<T>(this HttpClient http, string url, CancellationToken ct = default)
        {
            var response = await ExecuteAsync(() => http.GetAsync(url, ct), ct);

            var result = await HandleAsync<IReadOnlyList<T>>(response, ct);

            return result ?? [];
        }

        // ─────────────────────────────────────────────
        // POST (with response)
        // ─────────────────────────────────────────────
        public static async Task<T?> PostApiAsync<T>(this HttpClient http, string url, object body, CancellationToken ct = default)
        {
            var response = await ExecuteAsync(() => http.PostAsJsonAsync(url, body, ct), ct);
            return await HandleAsync<T>(response, ct);
        }

        // ─────────────────────────────────────────────
        // POST (no response body)
        // ─────────────────────────────────────────────
        public static async Task PostApiAsync(this HttpClient http, string url, CancellationToken ct = default)
        {
            var response = await ExecuteAsync(() => http.PostAsync(url, null, ct), ct);
            await HandleAsync<object>(response, ct);
        }

        // ─────────────────────────────────────────────
        // PUT
        // ─────────────────────────────────────────────
        public static async Task PutApiAsync<T>(this HttpClient http, string url, T body, CancellationToken ct = default)
        {
            var response = await ExecuteAsync(() => http.PutAsJsonAsync(url, body, ct), ct);
            await HandleAsync<object>(response, ct);
        }

        // ─────────────────────────────────────────────
        // PATCH (no body)
        // ─────────────────────────────────────────────
        public static async Task PatchApiAsync(this HttpClient http, string url, CancellationToken ct = default)
        {
            var response = await ExecuteAsync(() => http.PatchAsync(url, null, ct), ct);
            await HandleAsync<object>(response, ct);
        }

        // ─────────────────────────────────────────────
        // PATCH (with body)
        // ─────────────────────────────────────────────
        public static async Task PatchApiAsync<T>(this HttpClient http, string url, T body, CancellationToken ct = default)
        {
            var response = await ExecuteAsync(() => http.PatchAsJsonAsync(url, body, ct), ct);
            await HandleAsync<object>(response, ct);
        }

        // ─────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────
        public static async Task DeleteApiAsync(this HttpClient http, string url, CancellationToken ct = default)
        {
            var response = await ExecuteAsync(() => http.DeleteAsync(url, ct), ct);
            await HandleAsync<object>(response, ct);
        }
    }
}
