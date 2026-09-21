using Claims.Application.Auditing;
using Claims.Infrastructure.Auditing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Claims.Tests.Services.AuditingServices;

public class AuditQueueTests
{
    private readonly ILogger<AuditQueue> _logger;

    public AuditQueueTests()
    {
        _logger = Substitute.For<ILogger<AuditQueue>>();
    }

    [Fact]
    public void TryEnqueue_WhenQueueNotFull_ShouldReturnTrueAndEnqueueMessage()
    {
        // Arrange
        var queue = new AuditQueue(_logger, capacity: 10);
        var message = new AuditMessage("claim-1", "Claim", "POST");

        // Act
        var result = queue.TryEnqueue(message);

        // Assert
        result.Should().BeTrue();
        queue.TryDequeue(out var dequeued).Should().BeTrue();
        dequeued.Should().Be(message);
    }

    [Fact]
    public void TryEnqueue_WhenQueueIsFull_ShouldReturnFalseAndNotBlock()
    {
        // Arrange
        var queue = new AuditQueue(_logger, capacity: 2);
        queue.TryEnqueue(new AuditMessage("1", "Claim", "POST"));
        queue.TryEnqueue(new AuditMessage("2", "Claim", "POST"));

        // Act - this 3rd item should not fit
        var result = queue.TryEnqueue(new AuditMessage("3", "Claim", "POST"));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EnqueueAsync_WhenQueueIsFull_ShouldCompleteImmediatelyWithoutBlocking()
    {
        // Arrange
        var queue = new AuditQueue(_logger, capacity: 1);
        queue.TryEnqueue(new AuditMessage("1", "Claim", "POST"));

        // Act - EnqueueAsync on full queue must NOT wait or push backpressure onto request thread
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var enqueueTask = queue.EnqueueAsync(new AuditMessage("2", "Claim", "POST"), cts.Token);

        // Assert - must complete synchronously / immediately
        enqueueTask.IsCompleted.Should().BeTrue();
        await enqueueTask; // Should not throw OperationCanceledException
    }

    [Fact]
    public async Task DequeueBatchAsync_WhenMessagesAvailable_ShouldReturnBatchUpToMaxBatchSize()
    {
        // Arrange
        var queue = new AuditQueue(_logger, capacity: 10);
        for (int i = 0; i < 5; i++)
        {
            queue.TryEnqueue(new AuditMessage($"id-{i}", "Claim", "POST"));
        }

        // Act
        var batch = await queue.DequeueBatchAsync(maxBatchSize: 3, CancellationToken.None);

        // Assert
        batch.Should().HaveCount(3);
        batch[0].EntityId.Should().Be("id-0");
        batch[1].EntityId.Should().Be("id-1");
        batch[2].EntityId.Should().Be("id-2");

        // Remaining 2 items are still in queue
        var remainingBatch = await queue.DequeueBatchAsync(maxBatchSize: 10, CancellationToken.None);
        remainingBatch.Should().HaveCount(2);
        remainingBatch[0].EntityId.Should().Be("id-3");
        remainingBatch[1].EntityId.Should().Be("id-4");
    }

    [Fact]
    public async Task DequeueBatchAsync_WithSingleMessage_ShouldReturnImmediatelyWithoutWaitingForMaxBatch()
    {
        // Arrange
        var queue = new AuditQueue(_logger, capacity: 10);
        queue.TryEnqueue(new AuditMessage("claim-single", "Claim", "POST"));

        // Act - requested max 100, but only 1 is available
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var batch = await queue.DequeueBatchAsync(maxBatchSize: 100, cts.Token);

        // Assert - returns immediately with 1 item
        batch.Should().HaveCount(1);
        batch[0].EntityId.Should().Be("claim-single");
    }

    [Fact]
    public void TryDequeue_WhenQueueEmpty_ShouldReturnFalse()
    {
        // Arrange
        var queue = new AuditQueue(_logger, capacity: 10);

        // Act
        var result = queue.TryDequeue(out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().BeNull();
    }

    [Fact]
    public void TryEnqueue_NullMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        var queue = new AuditQueue(_logger, capacity: 10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => queue.TryEnqueue(null!));
    }
}
