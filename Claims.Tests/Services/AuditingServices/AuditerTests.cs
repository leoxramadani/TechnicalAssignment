using Claims.Application.Auditing;
using Claims.Infrastructure.Auditing;
using Claims.Persistance;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Claims.Tests.Services.AuditingServices;

public class AuditerTests
{
    private readonly AuditContext _auditContext;
    private readonly Auditer _sut;

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public AuditerTests()
    {
        var options = new DbContextOptionsBuilder<AuditContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _auditContext = new AuditContext(options);
        _sut = new Auditer(_auditContext);
    }

    [Fact]
    public async Task AuditBatchAsync_WithClaimMessages_ShouldPersistAllClaimAudits()
    {
        // Arrange
        var messages = new[]
        {
            new AuditMessage("claim-1", "Claim", "POST"),
            new AuditMessage("claim-2", "Claim", "DELETE")
        };

        // Act
        await _sut.AuditBatchAsync(messages, CancellationToken);

        // Assert
        var audits = await _auditContext.ClaimAudits.ToListAsync(CancellationToken);
        audits.Should().HaveCount(2);
        audits.Should().ContainSingle(a => a.ClaimId == "claim-1" && a.HttpRequestType == "POST");
        audits.Should().ContainSingle(a => a.ClaimId == "claim-2" && a.HttpRequestType == "DELETE");
    }

    [Fact]
    public async Task AuditBatchAsync_WithCoverMessages_ShouldPersistAllCoverAudits()
    {
        // Arrange
        var messages = new[]
        {
            new AuditMessage("cover-1", "Cover", "POST"),
            new AuditMessage("cover-2", "Cover", "DELETE")
        };

        // Act
        await _sut.AuditBatchAsync(messages, CancellationToken);

        // Assert
        var audits = await _auditContext.CoverAudits.ToListAsync(CancellationToken);
        audits.Should().HaveCount(2);
        audits.Should().ContainSingle(a => a.CoverId == "cover-1" && a.HttpRequestType == "POST");
        audits.Should().ContainSingle(a => a.CoverId == "cover-2" && a.HttpRequestType == "DELETE");
    }

    [Fact]
    public async Task AuditBatchAsync_WithMixedMessages_ShouldPersistBothTypesInSingleBatch()
    {
        // Arrange
        var timestamp = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var messages = new[]
        {
            new AuditMessage("claim-1", "Claim", "POST", timestamp),
            new AuditMessage("cover-1", "Cover", "POST", timestamp)
        };

        // Act
        await _sut.AuditBatchAsync(messages, CancellationToken);

        // Assert
        var claimAudits = await _auditContext.ClaimAudits.ToListAsync(CancellationToken);
        var coverAudits = await _auditContext.CoverAudits.ToListAsync(CancellationToken);

        claimAudits.Should().HaveCount(1);
        claimAudits[0].ClaimId.Should().Be("claim-1");
        claimAudits[0].Created.Should().Be(timestamp);

        coverAudits.Should().HaveCount(1);
        coverAudits[0].CoverId.Should().Be("cover-1");
        coverAudits[0].Created.Should().Be(timestamp);
    }

    [Fact]
    public async Task AuditBatchAsync_WithEmptyList_ShouldNotPerformDatabaseOperations()
    {
        // Act
        await _sut.AuditBatchAsync([], CancellationToken);

        // Assert
        (await _auditContext.ClaimAudits.CountAsync(CancellationToken)).Should().Be(0);
        (await _auditContext.CoverAudits.CountAsync(CancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task AuditBatchAsync_WithUnsupportedEntityType_ShouldThrowArgumentException()
    {
        // Arrange
        var messages = new[]
        {
            new AuditMessage("entity-1", "UnsupportedType", "POST")
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AuditBatchAsync(messages, CancellationToken));
    }

    [Fact]
    public async Task AuditAsync_SingleMessage_ShouldPersistAuditRecord()
    {
        // Arrange
        var message = new AuditMessage("claim-single", "Claim", "POST");

        // Act
        await _sut.AuditAsync(message, CancellationToken);

        // Assert
        var audits = await _auditContext.ClaimAudits.ToListAsync(CancellationToken);
        audits.Should().HaveCount(1);
        audits[0].ClaimId.Should().Be("claim-single");
    }
}
