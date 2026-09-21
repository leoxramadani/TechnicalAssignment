using Claims.Application.Auditing;
using Claims.Application.Claims;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Claims.Tests.Services.ClaimServices
{
    public class ClaimServiceTest
    {

        private readonly ClaimsContext _context;
        private readonly ClaimsService _sut;
        private readonly IAuditService _auditService;
        private readonly IAuditQueue _auditQueue;
        private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

        public ClaimServiceTest()
        {
            var options = new DbContextOptionsBuilder<ClaimsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ClaimsContext(options);

            _auditService = Substitute.For<IAuditService>();
            _auditQueue = Substitute.For<IAuditQueue>();

            _sut = new ClaimsService(
                _context,
                _auditQueue);
        }
        //get tests

        [Fact]
        public async Task GetClaimsAsync_WhenThereAreNoClaims_ShouldReturnEmptyCollection()
        {
            // Act
            var result = await _sut.GetClaimsAsync(CancellationToken);

            // Assert
            result.Should().BeEmpty();
        }
        [Fact]
        public async Task GetClaimsAsync_ShouldReturnAllClaims()
        {
            // Arrange
            var claims = new[]
            {
                new Claim
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Claim 1",
                    CoverId = "Cover1",
                    Created = DateTime.UtcNow,
                    DamageCost = 0,
                    Type = ClaimTypeEnum.BadWeather
                },
                new Claim
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Claim 2",
                    CoverId = "Cover2",
                    Created = DateTime.UtcNow,
                    DamageCost = 1,
                    Type = ClaimTypeEnum.Collision
                }
            };

            _context.Claims.AddRange(claims);
            await _context.SaveChangesAsync(CancellationToken);

            // Act
            var result = await _sut.GetClaimsAsync(CancellationToken);

            // Assert
            var resultList = result.ToList();

            resultList.Should().HaveCount(2);
            resultList.Should().ContainSingle(c => c.Id == claims[0].Id);
            resultList.Should().ContainSingle(c => c.Id == claims[1].Id);
        }
        [Fact]
        public async Task GetClaimsAsync_ShouldRespectCancellationToken()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => _sut.GetClaimsAsync(cancellationTokenSource.Token));
        }

        //create tests
        [Fact]
        public async Task CreateClaimAsync_ShouldPersistClaim()
        {
            // Arrange
            var claim = new Claim
            {
                Id = string.Empty,
                Name = "Test Claim",
                CoverId = "TestCover",
                Created = DateTime.UtcNow,
                DamageCost = 100,
                Type = ClaimTypeEnum.Collision
            };

            // Act
            var result = await _sut.CreateClaimAsync(claim, CancellationToken);

            // Assert
            var persistedClaim = await _context.Claims
                .SingleAsync(c => c.Id == result.Id, CancellationToken);

            persistedClaim.Should().NotBeNull();
            persistedClaim.Id.Should().Be(result.Id);
        }

        [Fact]
        public async Task CreateClaimAsync_ShouldGenerateNewId()
        {
            // Arrange
            var claim = new Claim
            {
                Id = "existing-id",
                Name = "Test Claim",
                CoverId = "TestCover",
                Created = DateTime.UtcNow,
                DamageCost = 100,
                Type = ClaimTypeEnum.Collision
            };

            // Act
            var result = await _sut.CreateClaimAsync(claim, CancellationToken);

            // Assert
            result.Id.Should().NotBeNullOrEmpty();
            result.Id.Should().NotBe("existing-id");
            Guid.TryParse(result.Id, out _).Should().BeTrue();
        }


        [Fact]
        public async Task CreateClaimAsync_ShouldReturnCreatedClaim()
        {
            // Arrange
            var claim = new Claim
            {
                Id = String.Empty,
                Name = "Test Claim",
                CoverId = "TestCover",
                Created = DateTime.UtcNow,
                DamageCost = 100,
                Type = ClaimTypeEnum.Collision
            };

            // Act
            var result = await _sut.CreateClaimAsync(claim, CancellationToken);

            // Assert
            result.Should().BeSameAs(claim);
            result.Id.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateClaimAsync_ShouldEnqueueAuditMessage()
        {
            // Arrange
            var claim = new Claim
            {
                Id = String.Empty,
                Name = "Test Claim",
                CoverId = "TestCover",
                Created = DateTime.UtcNow,
                DamageCost = 100,
                Type = ClaimTypeEnum.Collision
            };

            // Act
            await _sut.CreateClaimAsync(claim, CancellationToken);

            // Assert
            await _auditQueue.Received(1).EnqueueAsync(
                Arg.Is<AuditMessage>(message =>
                    message.EntityId == claim.Id &&
                    message.EntityType == "Claim" &&
                    message.HttpRequestType == "POST"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateClaimAsync_ShouldGenerateUniqueIdsForDifferentClaims()
        {
            // Arrange
            var claim1 = new Claim
            {
                Id = String.Empty,
                Name = "Test Claim",
                CoverId = "TestCover",
                Created = DateTime.UtcNow,
                DamageCost = 100,
                Type = ClaimTypeEnum.Collision
            };

            var claim2 = new Claim
            {
                Id = String.Empty,
                Name = "Test Claim2",
                CoverId = "TestCover2",
                Created = DateTime.UtcNow,
                DamageCost = 100,
                Type = ClaimTypeEnum.Collision
            };

            // Act
            var result1 = await _sut.CreateClaimAsync(claim1, CancellationToken);
            var result2 = await _sut.CreateClaimAsync(claim2, CancellationToken);

            // Assert
            result1.Id.Should().NotBe(result2.Id);
        }

        //delete claim

        [Fact]
        public async Task DeleteClaimAsync()
        {
            var claim1 = new Claim
            {
                Id = String.Empty,
                Name = "Test Claim",
                CoverId = "TestCover",
                Created = DateTime.UtcNow,
                DamageCost = 100,
                Type = ClaimTypeEnum.Collision
            };

            _context.Claims.Add(claim1);
            await _context.SaveChangesAsync(CancellationToken);

            var result = await _sut.DeleteClaimAsync(claim1.Id, CancellationToken);

            result.Should().BeTrue();

            var deletedClaim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claim1.Id, CancellationToken);
            deletedClaim.Should().BeNull();
        }

        //GetClaimById
        [Fact]
        public async Task GetClaimByIdAsync_ShouldReturnClaim()
        {
            // Arrange
            var claim = new Claim
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Claim",
                CoverId = "TestCover",
                Created = DateTime.UtcNow,
                DamageCost = 100,
                Type = ClaimTypeEnum.Collision
            };
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync(CancellationToken);
            // Act
            var result = await _sut.GetClaimByIdAsync(claim.Id, CancellationToken);
            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(claim.Id);
        }
    }
}
