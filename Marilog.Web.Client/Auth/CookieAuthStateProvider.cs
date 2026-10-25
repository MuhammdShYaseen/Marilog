using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Marilog.Web.Client.Auth
{
    public sealed class CookieAuthStateProvider : AuthenticationStateProvider, IDisposable
    {
        public const string FullNameClaim = "full_name";
        public const string SuperAdminRole = PermissionClaims.SuperAdminRole;

        private static readonly AuthenticationState Anonymous =
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        private readonly IAuthService _auth;
        private readonly AuthSessionEvents _events;
        private Task<AuthenticationState>? _cached;

        public CookieAuthStateProvider(IAuthService auth, AuthSessionEvents events)
        {
            _auth = auth;
            _events = events;
            _events.SessionExpired += OnSessionExpired;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => _cached ??= LoadAsync();

        public async Task LoginAsync(LoginRequest request)
        {
            var result = await _auth.LoginAsync(request);
            if (result != LoginResult.Success)
                throw new InvalidOperationException("Login failed.");

            _cached = LoadAsync();
            NotifyAuthenticationStateChanged(_cached);

            var state = await _cached;
            if (state.User.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException(
                    "Login succeeded but the browser did not send the session cookie back.");
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _auth.LogoutAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Logout request failed: {ex.Message}");
            }

            SetAnonymous();
        }

        private void OnSessionExpired()
        {
            if (_cached is { IsCompletedSuccessfully: true } t
                && t.Result.User.Identity?.IsAuthenticated == true)
                SetAnonymous();
        }

        private void SetAnonymous()
        {
            _cached = Task.FromResult(Anonymous);
            NotifyAuthenticationStateChanged(_cached);
        }

        private async Task<AuthenticationState> LoadAsync()
        {
            CurrentUserResponse? user;
            try
            {
                user = await _auth.GetCurrentUserAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Loading current user failed: {ex.Message}");
                return Anonymous;
            }

            if (user is null) return Anonymous;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName)
            };
            if (!string.IsNullOrWhiteSpace(user.FullName))
                claims.Add(new(FullNameClaim, user.FullName));
            if (user.IsSuperAdmin)
                claims.Add(new(ClaimTypes.Role, SuperAdminRole));
            if (user.Permissions is not null)
            {
                foreach (var p in user.Permissions)
                    claims.Add(new(PermissionClaims.ClaimType, PermissionClaims.ToValue(p.Kind, p.Value, p.Actions)));
            }
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookie")));
        }

        public void Dispose() => _events.SessionExpired -= OnSessionExpired;
    }
}