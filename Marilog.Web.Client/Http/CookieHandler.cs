using System.Net;
using Marilog.Web.Client.Auth;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Marilog.Web.Client.Http
{
    public sealed class CookieHandler(AuthSessionEvents events) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !IsAuthEndpoint(request.RequestUri))
                events.RaiseSessionExpired();

            return response;
        }

        private static bool IsAuthEndpoint(Uri? uri)
            => uri is not null
               && uri.AbsolutePath.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase);
    }
}