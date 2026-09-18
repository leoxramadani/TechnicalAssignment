using Claims.Controllers;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Services.CoversServices;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


namespace Claims.Tests.Controllers
{
    public class CoversControllerTests
    {

        private readonly Mock<ILogger<CoversController>> _loggerMock;
        private readonly Mock<ICoversService> _coversService;
        private readonly Mock<IValidator<Cover>> _validatorMock;
        private readonly CoversController _controller;

        public CoversControllerTests()
        {
            _loggerMock = new Mock<ILogger<CoversController>>();
            _validatorMock = new Mock<IValidator<Cover>>();
            _coversService = new Mock<ICoversService>();

            _controller = new CoversController(
                _loggerMock.Object,
                _coversService.Object,
                _validatorMock.Object);
        }


        [Fact]
        public async Task GetAsync_ShouldReturnAllCovers()
        {
            var covers = new List<Cover>
            {
                new() { Id = "1",Premium = 100, StartDate=DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) },
                new() { Id = "2",Premium = 200, StartDate=DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) }
            };


            _coversService
                .Setup(service => service.GetCoversAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(covers);

            var result = await _controller.GetAsync(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCovers = Assert.IsType<List<Cover>>(okResult.Value);


            Assert.Equal(2, returnedCovers.Count);
            Assert.Equal(covers, returnedCovers);

            _coversService.Verify(srv => srv.GetCoversAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnEmptyList()
        {
            // Arrange
            _coversService
                .Setup(service => service.GetCoversAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            // Act
            var result = await _controller.GetAsync(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCovers = Assert.IsType<IEnumerable<Cover>>(okResult.Value, exactMatch: false);

            Assert.Empty(returnedCovers);

            _coversService.Verify(
                service => service.GetCoversAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_ById_ShouldReturnOk()
        {
            var covers = new Cover { Id = "1", Premium = 100, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) };

            _coversService
                .Setup(service => service.GetCoverByIdAsync(
                    covers.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(covers);

            var result = await _controller.GetAsync(
                covers.Id,
                CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCover = Assert.IsType<Cover>(okResult.Value);

            Assert.Equal(covers.Id, returnedCover.Id);
            Assert.Equal(covers.Premium, returnedCover.Premium);
            Assert.Equal(covers.StartDate, returnedCover.StartDate);
            Assert.Equal(covers.EndDate, returnedCover.EndDate);

            _coversService.Verify(
                service => service.GetCoverByIdAsync(
                    covers.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_ById_ShouldReturnNotFound()
        {
            // Arrange
            const string coverId = "dummyId";

            _coversService
                .Setup(service => service.GetCoverByIdAsync(
                    coverId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cover?)null);

            // Act
            var result = await _controller.GetAsync(
                coverId,
                CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);

            _coversService.Verify(
                service => service.GetCoverByIdAsync(
                    coverId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenCoverIsValid_ShouldReturnCreated()
        {
            // Arrange
            var cover = new Cover
            {
                Id = "1",
                Type = CoverTypeEnum.Yacht,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                Premium = 1000
            };

            var createdCover = new Cover
            {
                Id = "1",
                Type = CoverTypeEnum.Tanker,
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,
                Premium = cover.Premium
            };

            _validatorMock
                .Setup(validator => validator.ValidateAsync(
                    cover,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _coversService
                .Setup(service => service.CreateCoverAsync(
                    cover,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdCover);

            // Act
            var result = await _controller.CreateAsync(
                cover,
                CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);

            Assert.Equal("GetCoverById", createdResult.RouteName);

            var returnedCover = Assert.IsType<Cover>(createdResult.Value);

            Assert.Equal(createdCover.Id, returnedCover.Id);
            Assert.Equal(createdCover.Type, returnedCover.Type);
            Assert.Equal(createdCover.Premium, returnedCover.Premium);

            _validatorMock.Verify(
                validator => validator.ValidateAsync(
                    cover,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _coversService.Verify(
                service => service.CreateCoverAsync(
                    cover,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenCoverExists_ShouldReturnNoContent()
        {
            // Arrange
            const string coverId = "cover-1";

            _coversService
                .Setup(service => service.DeleteCoverAsync(
                    coverId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAsync(
                coverId,
                CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);

            _coversService.Verify(
                service => service.DeleteCoverAsync(
                    coverId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

    }
}
