using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Helpers.PremiumComputation;
using Claims.Persistance;
using Claims.Services.AuditingServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Claims.Tests.Services.CoverServices
{
    public class CoversServiceTest
    {
        private readonly ClaimsContext _context;
        private readonly IPremiumComputationService _premiumComputationService;
        private readonly Claims.Services.CoversServices.CoversService _sut;
        private readonly IAuditQueue _auditQueue;
        private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

        public CoversServiceTest()
        {
            var options = new DbContextOptionsBuilder<ClaimsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ClaimsContext(options);

            _premiumComputationService = Substitute.For<IPremiumComputationService>();
            _auditQueue = Substitute.For<IAuditQueue>();

            _sut = new Claims.Services.CoversServices.CoversService(
                _context,
                _premiumComputationService,
                _auditQueue);
        }


        [Fact]
        public async Task GetCoversAsync_NoClaims_ShouldReturnEmptyCollection()
        {
            var result = await _sut.GetCoversAsync(CancellationToken);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCoversAsync_ShouldReturnAllCovers()
        {
            var covers = new[]
            {
                new Cover
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = CoverTypeEnum.Tanker,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    Premium = 100
                },
                new Cover
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = CoverTypeEnum.Yacht,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    Premium = 200
                }
            };
            await _context.Covers.AddRangeAsync(covers);
            await _context.SaveChangesAsync(CancellationToken);
            var result = await _sut.GetCoversAsync(CancellationToken);
            result.Should().HaveCount(2);
        }
        [Fact]
        public async Task GetCoversAsync_ShouldRespectCancellationToken()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.GetCoversAsync(cancellationTokenSource.Token));
        }

        //create
        [Fact]
        public async Task CreateCoversAsync_ShouldPersistCover()
        {
            var covers =
                new Cover
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = CoverTypeEnum.Tanker,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    Premium = 100
                };

            _premiumComputationService.ComputePremium(
                covers.StartDate,
                covers.EndDate,
                covers.Type).Returns(150);

            var result = await _sut.CreateCoverAsync(covers, CancellationToken);

            await _auditQueue.Received(1).EnqueueAsync(
                Arg.Is<AuditMessage>(message =>
                    message.EntityId == covers.Id &&
                    message.EntityType == "Cover" &&
                    message.HttpRequestType == "POST"),
                Arg.Any<CancellationToken>());

            var persistedCover = await _context.Covers.SingleAsync(c => c.Id == result.Id, CancellationToken);
            
            persistedCover.Should().NotBeNull();
            persistedCover.Id.Should().Be(result.Id);
            persistedCover.Premium.Should().Be(150);
        }

        [Fact]
        public async Task CreateCoverAsync_ShouldGenerateUniqueIdsForDifferentCovers()
        {
            var cover1 = new Cover
            {
                Id = string.Empty,
                Type = CoverTypeEnum.Tanker,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Premium = 100
            };
            var cover2 = new Cover
            {
                Id = string.Empty,
                Type = CoverTypeEnum.Yacht,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Premium = 200
            };
            var result1 = await _sut.CreateCoverAsync(cover1, CancellationToken);
            var result2 = await _sut.CreateCoverAsync(cover2, CancellationToken);
            result1.Id.Should().NotBe(result2.Id);
        }

        //delete cover
        [Fact]
        public async Task DeleteCoverAsync()
        {
            var cover1 = new Cover
            {
                Id = string.Empty,
                Type = CoverTypeEnum.Tanker,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Premium = 100
            };

            _context.Covers.Add(cover1);
            await _context.SaveChangesAsync(CancellationToken);

            var result = await _sut.DeleteCoverAsync(cover1.Id, CancellationToken);

            result.Should().BeTrue();

            var deletedCover = await _context.Covers.FirstOrDefaultAsync(c => c.Id == cover1.Id, CancellationToken);
            deletedCover.Should().BeNull();
        }

        [Fact]
        public async Task GetCoverById()
        {
            var cover1 = new Cover
            {
                Id = string.Empty,
                Type = CoverTypeEnum.Tanker,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Premium = 100
            };
            _context.Covers.Add(cover1);
            await _context.SaveChangesAsync(CancellationToken);

            var result = await _sut.GetCoverByIdAsync(cover1.Id, CancellationToken);

            result.Should().NotBeNull();
            result.Id.Should().Be(cover1.Id);
        }

    }
}
