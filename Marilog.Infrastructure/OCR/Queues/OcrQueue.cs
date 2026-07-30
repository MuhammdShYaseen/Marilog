using Marilog.Contracts.DTOs.OCR;
using System.Threading.Channels;

namespace Marilog.Infrastructure.OCR.Queues
{
    public sealed class OcrQueue
    {
        private readonly Channel<OcrRequest> _channel =
            Channel.CreateUnbounded<OcrRequest>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        public ChannelReader<OcrRequest> Reader => _channel.Reader;
        public ValueTask EnqueueAsync(OcrRequest request, CancellationToken ct = default)
            => _channel.Writer.WriteAsync(request, ct);
    }
}
