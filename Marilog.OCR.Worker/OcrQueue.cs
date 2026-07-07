using System.Threading.Channels;

namespace Marilog.OCR.Worker;

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

public sealed record OcrRequest 
{
    public int DocumentId { get; set; }
    public string FilePath { get; set; } = string.Empty;
}