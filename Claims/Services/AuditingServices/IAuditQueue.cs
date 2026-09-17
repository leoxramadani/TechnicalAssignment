using System.Threading.Channels;

namespace Claims.Services.AuditingServices;

public interface IAuditQueue
{
    ValueTask EnqueueAsync(
        AuditMessage message,
        CancellationToken cancellationToken = default);

    ValueTask<AuditMessage> DequeueAsync(
        CancellationToken cancellationToken);
}

public sealed class AuditQueue : IAuditQueue
{
    private readonly Channel<AuditMessage> _queue;

    public AuditQueue()
    {
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _queue = Channel.CreateBounded<AuditMessage>(options);
    }

    public ValueTask EnqueueAsync(
        AuditMessage message,
        CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(message, cancellationToken);
    }

    public ValueTask<AuditMessage> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}