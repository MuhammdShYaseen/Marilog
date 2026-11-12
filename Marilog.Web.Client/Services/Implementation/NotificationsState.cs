using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.NotifiServices;

namespace Marilog.Web.Client.Services.Implementation
{
    // The bell: unread count and the latest notifications.
    // Refreshed when the bell loads, every 5 minutes, and after the user reads one.
    public sealed class NotificationsState : IDisposable
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        private readonly INotificationService _notifications;
        private Timer? _timer;

        public NotificationsState(INotificationService notifications) => _notifications = notifications;

        public int UnreadCount { get; private set; }
        public IReadOnlyList<NotificationResponse> Items { get; private set; } = [];

        public event Action? Changed;

        public void Start()
            => _timer ??= new Timer(_ => _ = RefreshAsync(), null, TimeSpan.Zero, Interval);

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
            UnreadCount = 0;
            Items = [];
        }

        public async Task RefreshAsync()
        {
            try
            {
                UnreadCount = await _notifications.GetUnreadCountAsync();
                Items = await _notifications.GetMineAsync();
                Changed?.Invoke();
            }
            catch (Exception ex)
            {
                // The bell is a convenience; a failed refresh keeps the last state.
                Console.WriteLine($"Notifications refresh failed: {ex.Message}");
            }
        }

        public async Task MarkReadAsync(long id)
        {
            try { await _notifications.MarkReadAsync(id); }
            catch (Exception ex) { Console.WriteLine($"Mark read failed: {ex.Message}"); }
            await RefreshAsync();
        }

        public async Task MarkAllReadAsync()
        {
            try { await _notifications.MarkAllReadAsync(); }
            catch (Exception ex) { Console.WriteLine($"Mark all read failed: {ex.Message}"); }
            await RefreshAsync();
        }

        public void Dispose() => Stop();
    }
}