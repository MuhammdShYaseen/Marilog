using Marilog.Contracts.DTOs.OCR;
using Marilog.Contracts.Interfaces.OCR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace Marilog.Infrastructure
{
    public sealed class OcrQueue : IOcrQueue, IDisposable
    {
        private readonly Channel<OcrRequest> _channel;

        public OcrQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };

            _channel = Channel.CreateBounded<OcrRequest>(options);
        }

        public ValueTask EnqueueAsync(OcrRequest request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new ValueTask(_channel.Writer.WriteAsync(request, ct).AsTask());
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
        }
    }
}
