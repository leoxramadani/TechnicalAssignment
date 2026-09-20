using Claims.Services.AuditingServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Claims.Tests.Services.AuditingServices;

public class AuditBackgroundServiceTests
{
    private readonly IAuditQueue _auditQueue;
    private readonly IAuditService _auditService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger;

    public AuditBackgroundServiceTests()
    {
        _auditQueue = Substitute.For<IAuditQueue>();
        _auditService = Substitute.For<IAuditService>();
        _logger = Substitute.For<ILogger<AuditBackgroundService>>();

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAuditService)).Returns(_auditService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(scope);
    }

    private sealed class TestableAuditBackgroundService(
        IAuditQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditBackgroundService> logger)
        : AuditBackgroundService(queue, scopeFactory, logger)
    {
        public Task RunExecuteAsync(CancellationToken stoppingToken) => ExecuteAsync(stoppingToken);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBatchAvailable_ShouldInvokeAuditBatchAsync()
    {
        // Arrange
        var batch = new List<AuditMessage>
        {
            new("claim-1", "Claim", "POST"),
            new("cover-1", "Cover", "POST")
        };

        using var cts = new CancellationTokenSource();

        _auditQueue.DequeueBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                callInfo =>
                {
                    cts.Cancel(); // Cancel after first batch to exit loop
                    return ValueTask.FromResult<IReadOnlyList<AuditMessage>>(batch);
                });

        var sut = new TestableAuditBackgroundService(_auditQueue, _scopeFactory, _logger);

        // Act
        await sut.RunExecuteAsync(cts.Token);

        // Assert
        await _auditService.Received(1).AuditBatchAsync(
            Arg.Is<IReadOnlyList<AuditMessage>>(b => b.Count == 2 && b[0].EntityId == "claim-1" && b[1].EntityId == "cover-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_OnShutdown_ShouldDrainRemainingMessages()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Immediately cancelled

        var remainingMessage = new AuditMessage("claim-remaining", "Claim", "POST");
        bool hasReturned = false;

        _auditQueue.TryDequeue(out Arg.Any<AuditMessage>()!)
            .Returns(callInfo =>
            {
                if (!hasReturned)
                {
                    hasReturned = true;
                    callInfo[0] = remainingMessage;
                    return true;
                }
                callInfo[0] = null!;
                return false;
            });

        var sut = new TestableAuditBackgroundService(_auditQueue, _scopeFactory, _logger);

        // Act
        await sut.RunExecuteAsync(cts.Token);

        // Assert - remaining message must have been processed during drain
        await _auditService.Received().AuditBatchAsync(
            Arg.Is<IReadOnlyList<AuditMessage>>(b => b.Any(m => m.EntityId == "claim-remaining")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenBatchThrows_ShouldLogErrorAndContinue()
    {
        // Arrange
        var batch = new List<AuditMessage>
        {
            new("claim-err", "Claim", "POST")
        };

        using var cts = new CancellationTokenSource();
        int callCount = 0;

        _auditQueue.DequeueBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callCount++;
                if (callCount >= 2)
                {
                    cts.Cancel();
                }
                return ValueTask.FromResult<IReadOnlyList<AuditMessage>>(batch);
            });

        _auditService.AuditBatchAsync(Arg.Any<IReadOnlyList<AuditMessage>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("DB connection error")));

        var sut = new TestableAuditBackgroundService(_auditQueue, _scopeFactory, _logger);

        // Act - should not throw unhandled exception
        var act = async () =>
        {
            await sut.RunExecuteAsync(cts.Token);
        };

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_OnShutdown_WhenExceedBatchLimit_ShouldProcessInMultipleBatches()
    {
        // Arrange
        var totalMessages = AuditBackgroundService.DefaultBatchSize + 10; // exceed by 10
        var messages = Enumerable.Range(0, totalMessages)
            .Select(i => new AuditMessage($"claim-{i}", "Claim", "POST"))
            .ToArray();

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // trigger immediate shutdown so DrainRemainingAuditsAsync runs

        int index = 0;
        _auditQueue.TryDequeue(out Arg.Any<AuditMessage>()!)
            .Returns(callInfo =>
            {
                if (index < messages.Length)
                {
                    callInfo[0] = messages[index++];
                    return true;
                }
                callInfo[0] = null!;
                return false;
            });

        var sut = new TestableAuditBackgroundService(_auditQueue, _scopeFactory, _logger);

        // Act
        await sut.RunExecuteAsync(cts.Token);

        // Assert
        var expectedBatches = (totalMessages + AuditBackgroundService.DefaultBatchSize - 1) / AuditBackgroundService.DefaultBatchSize;
        await _auditService.Received(expectedBatches).AuditBatchAsync(Arg.Any<IReadOnlyList<AuditMessage>>(), Arg.Any<CancellationToken>());
    }
}
