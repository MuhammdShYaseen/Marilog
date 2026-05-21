using Marilog.Application.Interfaces.Events;
using Marilog.Domain.Common;
using System.Threading.Channels;

namespace Marilog.Infrastructure.Dispatchers
{
    public class InMemoryEventDispatcher : IEventDispatcher, IDisposable
    {

        private readonly Channel<IDomainEvent> _channel;
        public InMemoryEventDispatcher(int capacity = 1000)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<IDomainEvent>(options);
        }

        public ChannelReader<IDomainEvent> Reader => _channel.Reader;

        public async Task EnqueueAsync(IDomainEvent @event, CancellationToken ct = default)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            await _channel.Writer.WriteAsync(@event, ct);
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
        }
    }
}