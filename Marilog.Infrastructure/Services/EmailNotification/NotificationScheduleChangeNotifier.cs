using Marilog.Infrastructure.Interfaces.EmailNotification;

namespace Marilog.Infrastructure.Services.EmailNotification
{
    public sealed class NotificationScheduleChangeNotifier : INotificationScheduleChangeNotifier
    {
        private CancellationTokenSource _changeCts = new();

        public void NotifyChanged()
        {
            var oldCts = Interlocked.Exchange(
                ref _changeCts,
                new CancellationTokenSource());

            oldCts.Cancel();
            oldCts.Dispose();
        }

        public async Task WaitForChangeAsync(CancellationToken cancellationToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _changeCts.Token);

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, linkedCts.Token);
            }
            catch (OperationCanceledException)
                when (_changeCts.IsCancellationRequested &&
                      !cancellationToken.IsCancellationRequested)
            {
                // Schedule changed.
            }
        }
    }
}
