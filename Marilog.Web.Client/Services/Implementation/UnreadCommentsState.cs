using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Shared.UI.Components.Comments;

namespace Marilog.Web.Client.Services.Implementation
{
    // Holds the signed-in user's unread comment counts.
    // Refreshed when the layout loads, every 5 minutes, and after a comments dialog closes.
    public sealed class UnreadCommentsState : IDisposable
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        private readonly ICommentService _comments;
        private Timer? _timer;

        public UnreadCommentsState(ICommentService comments) => _comments = comments;

        public UnreadComments Current { get; private set; } = UnreadComments.Empty;

        public event Action? Changed;

        public void Start()
            => _timer ??= new Timer(_ => _ = RefreshAsync(), null, TimeSpan.Zero, Interval);

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
            Current = UnreadComments.Empty;
        }

        public async Task RefreshAsync()
        {
            try
            {
                var items = await _comments.GetUnreadSummaryAsync();
                Current = new UnreadComments(items);
                Changed?.Invoke();
            }
            catch (Exception ex)
            {
                // Badges are a convenience; a failed refresh keeps the last counts.
                Console.WriteLine($"Unread comments refresh failed: {ex.Message}");
            }
        }

        public void Dispose() => Stop();
    }
}