using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Claims.Services.AuditingServices;

public interface IAuditQueue
{
    ValueTask EnqueueAsync(
        AuditMessage message,
        CancellationToken cancellationToken = default);

    bool TryEnqueue(AuditMessage message);

    ValueTask<AuditMessage> DequeueAsync(
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<AuditMessage>> DequeueBatchAsync(
        int maxBatchSize,
        CancellationToken cancellationToken = default);

    bool TryDequeue(out AuditMessage message);

    ValueTask<bool> WaitToReadAsync(
        CancellationToken cancellationToken = default);
}

public sealed class AuditQueue : IAuditQueue
{
    public const int DefaultCapacity = 10000;

    private readonly Channel<AuditMessage> _queue;
    private readonly ILogger<AuditQueue> _logger;
    private readonly int _capacity;

    public AuditQueue(ILogger<AuditQueue>? logger = null, int capacity = DefaultCapacity)
    {
        _logger = logger ?? NullLogger<AuditQueue>.Instance;
        _capacity = capacity > 0 ? capacity : DefaultCapacity;

        var options = new BoundedChannelOptions(_capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _queue = Channel.CreateBounded<AuditMessage>(options);
    }

    public bool TryEnqueue(AuditMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_queue.Writer.TryWrite(message))
        {
            return true;
        }

        _logger.LogWarning(
            "Audit queue is full (capacity {Capacity}). Dropping audit message for {EntityType} with ID {EntityId} to avoid backpressure on request thread.",
            _capacity,
            message.EntityType,
            message.EntityId);

        return false;
    }

    public ValueTask EnqueueAsync(
        AuditMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TryEnqueue(message);
        return ValueTask.CompletedTask;
    }

    public ValueTask<AuditMessage> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }

    public async ValueTask<IReadOnlyList<AuditMessage>> DequeueBatchAsync(
        int maxBatchSize,
        CancellationToken cancellationToken = default)
    {
        if (maxBatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBatchSize), "Batch size must be greater than zero.");
        }

        if (!await _queue.Reader.WaitToReadAsync(cancellationToken))
        {
            return [];
        }

        var batch = new List<AuditMessage>(maxBatchSize);
        while (batch.Count < maxBatchSize && _queue.Reader.TryRead(out var message))
        {
            batch.Add(message);
        }

        return batch;
    }

    public bool TryDequeue(out AuditMessage message)
    {
        return _queue.Reader.TryRead(out message!);
    }

    public ValueTask<bool> WaitToReadAsync(
        CancellationToken cancellationToken = default)
    {
        return _queue.Reader.WaitToReadAsync(cancellationToken);
    }
}