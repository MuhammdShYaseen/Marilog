namespace Marilog.Web.Client.Auth
{
    public sealed class AuthSessionEvents
    {
        public event Action? SessionExpired;
        public void RaiseSessionExpired() => SessionExpired?.Invoke();
    }
}
