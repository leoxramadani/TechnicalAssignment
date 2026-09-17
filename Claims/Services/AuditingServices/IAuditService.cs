namespace Claims.Services.AuditingServices
{
    public interface IAuditService
    {
        Task AuditAsync(
            AuditMessage message,
            CancellationToken cancellationToken = default);

    }
}
