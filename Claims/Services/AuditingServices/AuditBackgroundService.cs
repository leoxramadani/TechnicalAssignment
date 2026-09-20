namespace Claims.Services.AuditingServices;

public class AuditBackgroundService(
    IAuditQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<AuditBackgroundService> logger) : BackgroundService
{
    public const int DefaultBatchSize = 100;

    private readonly IAuditQueue _queue = queue;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var batch = await _queue.DequeueBatchAsync(DefaultBatchSize, stoppingToken);

                if (batch.Count > 0)
                {
                    await ProcessBatchAsync(batch, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing audit message batch.");
            }
        }

        await DrainRemainingAuditsAsync();
    }

    private async Task ProcessBatchAsync(IReadOnlyList<AuditMessage> batch, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var auditService =
            scope.ServiceProvider.GetRequiredService<IAuditService>();

        await auditService.AuditBatchAsync(batch, cancellationToken);
    }

    private async Task DrainRemainingAuditsAsync()
    {
        try
        {
            var remaining = new List<AuditMessage>();
            while (_queue.TryDequeue(out var message))
            {
                remaining.Add(message);
                if (remaining.Count >= DefaultBatchSize)
                {
                    using var shutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await ProcessBatchAsync(remaining, shutdownCts.Token);
                    remaining.Clear();
                }
            }

            if (remaining.Count > 0)
            {
                using var shutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await ProcessBatchAsync(remaining, shutdownCts.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while draining remaining audit messages during shutdown.");
        }
    }
}