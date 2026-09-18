namespace Claims.Services.AuditingServices;

public sealed class AuditBackgroundService(
    IAuditQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<AuditBackgroundService> logger) : BackgroundService
{
    private readonly IAuditQueue _queue = queue;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _queue.DequeueAsync(stoppingToken);

                await ProcessAuditAsync(message, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing an audit message.");
            }
        }
    }

    private async Task ProcessAuditAsync(AuditMessage message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var auditService =
            scope.ServiceProvider.GetRequiredService<IAuditService>();

    
        await auditService.AuditAsync(
                    message,
                    cancellationToken);
        
    }
}