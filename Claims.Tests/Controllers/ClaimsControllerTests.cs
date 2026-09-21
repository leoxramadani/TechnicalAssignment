using Claims.Api.Controllers;
using Claims.Application.Claims;
using Claims.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Claims.Tests.Controllers
{
    public class ClaimsControllerTests
    {
        private readonly Mock<ILogger<ClaimsController>> _loggerMock;
        private readonly Mock<IClaimsService> _claimsServiceMock;
        private readonly Mock<IValidator<Claim>> _validatorMock;
        private readonly ClaimsController _controller;

        public ClaimsControllerTests()
        {
            _loggerMock = new Mock<ILogger<ClaimsController>>();
            _claimsServiceMock = new Mock<IClaimsService>();
            _validatorMock = new Mock<IValidator<Claim>>();

            _controller = new ClaimsController(
                _loggerMock.Object,
                _claimsServiceMock.Object);
        }


        [Fact]
        public async Task GetAsync_ShouldReturnAllClaims()
        {
            var claims = new List<Claim>
            {
                new() { Id = "1", Name = "Claim 1", Created = DateTime.UtcNow , DamageCost = 50},
                new() { Id = "2", Name = "Claim 2", Created = DateTime.UtcNow , DamageCost = 100}
            };


            _claimsServiceMock
                .Setup(service => service.GetClaimsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(claims);

            var result = await _controller.GetAsync(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClaims = Assert.IsType<List<Claim>>(okResult.Value);


            Assert.Equal(2, returnedClaims.Count);
            Assert.Equal(claims, returnedClaims);

            _claimsServiceMock.Verify(srv => srv.GetClaimsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task GetAsync_ShouldReturnEmptyList()
        {
            _claimsServiceMock
                .Setup(service => service.GetClaimsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var result = await _controller.GetAsync(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClaims = Assert.IsType<List<Claim>>(okResult.Value);

            Assert.Empty(returnedClaims);

            _claimsServiceMock.Verify(
                service => service.GetClaimsAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task GetAsync_ById_ShouldReturnOk()
        {
            var claim = new Claim
            {
                Id = "claim1",
                CoverId = "cover1",
                Created = DateTime.UtcNow,
                DamageCost = 1500
            };

            _claimsServiceMock
                .Setup(service => service.GetClaimByIdAsync(
                    claim.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(claim);

            var result = await _controller.GetAsync(
                claim.Id,
                CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClaim = Assert.IsType<Claim>(okResult.Value);

            Assert.Equal(claim.Id, returnedClaim.Id);
            Assert.Equal(claim.CoverId, returnedClaim.CoverId);
            Assert.Equal(claim.DamageCost, returnedClaim.DamageCost);

            _claimsServiceMock.Verify(
                service => service.GetClaimByIdAsync(
                    claim.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_ById_ShouldReturnNotFound()
        {
            // Arrange
            const string claimId = "dummyId";

            _claimsServiceMock
                .Setup(service => service.GetClaimByIdAsync(
                    claimId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Claim?)null);

            // Act
            var result = await _controller.GetAsync(
                claimId,
                CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);

            _claimsServiceMock.Verify(
                service => service.GetClaimByIdAsync(
                    claimId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }



        [Fact]
        public async Task CreateAsync_ShouldReturnCreated()
        {
            var claim = new Claim
            {
                Id = "claim1",
                CoverId = "cover1",
                Created = DateTime.UtcNow,
                DamageCost = 1500
            };

            var createdClaim = new Claim
            {
                Id = "claim2",
                CoverId = "cover2",
                Created = claim.Created,
                DamageCost = 1500
            };
            _validatorMock
            .Setup(validator => validator.ValidateAsync(
                claim,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

            _claimsServiceMock
                .Setup(service => service.CreateClaimAsync(
                    claim,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClaim);


            var result = await _controller.CreateAsync(claim, CancellationToken.None);

            var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);

            Assert.Equal("GetClaimById", createdResult.RouteName);
           
            var returnedClaim = Assert.IsType<Claim>(createdResult.Value);
            Assert.Equal(createdClaim.Id, returnedClaim.Id);
            Assert.Equal(createdClaim.CoverId, returnedClaim.CoverId);
            Assert.Equal(createdClaim.DamageCost, returnedClaim.DamageCost);

            _claimsServiceMock.Verify(
                service => service.CreateClaimAsync(
                    claim,
                    It.IsAny<CancellationToken>()),
                Times.Once);

        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNoContent()
        {
            const string claimId = "claim1";

            _claimsServiceMock
                .Setup(service => service.DeleteClaimAsync(
                    claimId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.DeleteAsync(
                claimId,
                CancellationToken.None);

            Assert.IsType<NoContentResult>(result);

            _claimsServiceMock.Verify(
                service => service.DeleteClaimAsync(
                    claimId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
