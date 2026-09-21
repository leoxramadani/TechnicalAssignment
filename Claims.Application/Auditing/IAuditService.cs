namespace Claims.Application.Auditing;

public interface IAuditService
{
    Task AuditAsync(
        AuditMessage message,
        CancellationToken cancellationToken = default);

    Task AuditBatchAsync(
        IReadOnlyList<AuditMessage> messages,
        CancellationToken cancellationToken = default);
}
