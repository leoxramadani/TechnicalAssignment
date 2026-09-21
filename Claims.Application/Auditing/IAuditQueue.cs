namespace Claims.Application.Auditing;

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
