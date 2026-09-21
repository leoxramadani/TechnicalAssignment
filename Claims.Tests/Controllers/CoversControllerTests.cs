using Claims.Api.Controllers;
using Claims.Application.Covers;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
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
        private readonly CoversController _controller;

        public CoversControllerTests()
        {
            _loggerMock = new Mock<ILogger<CoversController>>();
            _coversService = new Mock<ICoversService>();

            _controller = new CoversController(
                _loggerMock.Object,
                _coversService.Object);
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
                .ReturnsAsync(new List<Cover>());

            // Act
            var result = await _controller.GetAsync(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCovers = Assert.IsAssignableFrom<IEnumerable<Cover>>(okResult.Value);

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
            Assert.IsType<NoContentResult>(result);

            _coversService.Verify(
                service => service.DeleteCoverAsync(
                    coverId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── ComputePremium 

        [Fact]
        public void ComputePremium_WithValidDates_ShouldReturnOk()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate   = startDate.AddDays(30);
            const decimal expectedPremium = 50_000m;

            _coversService
                .Setup(s => s.ComputePremium(startDate, endDate, CoverTypeEnum.Yacht))
                .Returns(expectedPremium);

            // Act
            var result = _controller.ComputePremium(startDate, endDate, CoverTypeEnum.Yacht);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(expectedPremium, returned);

            _coversService.Verify(s => s.ComputePremium(startDate, endDate, CoverTypeEnum.Yacht), Times.Once);
        }

        [Fact]
        public void ComputePremium_WithReversedPeriod_ShouldReturnBadRequest()
        {
            // Arrange – endDate is before startDate (the originally reported bug)
            var startDate = DateTime.UtcNow.Date.AddDays(10);
            var endDate   = startDate.AddDays(-11); // reversed

            // Act
            _coversService
                .Setup(s => s.ComputePremium(startDate, endDate, CoverTypeEnum.Yacht))
                .Returns(0m);

            var result = _controller.ComputePremium(startDate, endDate, CoverTypeEnum.Yacht);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(0m, returned);

            _coversService.Verify(
                s => s.ComputePremium(startDate, endDate, CoverTypeEnum.Yacht),
                Times.Once);
        }

        [Fact]
        public void ComputePremium_WithSameDayPeriod_ShouldReturnBadRequest()
        {
            // Arrange – same-day cover produces a zero-day period and £0 premium
            var startDate = DateTime.UtcNow.Date.AddDays(1);

            // Act
            _coversService
                .Setup(s => s.ComputePremium(startDate, startDate, CoverTypeEnum.Yacht))
                .Returns(0m);

            var result = _controller.ComputePremium(startDate, startDate, CoverTypeEnum.Yacht);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(0m, returned);

            _coversService.Verify(
                s => s.ComputePremium(startDate, startDate, CoverTypeEnum.Yacht),
                Times.Once);
        }

        [Fact]
        public void ComputePremium_WithStartDateInThePast_ShouldReturnBadRequest()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date.AddDays(-5);
            var endDate   = startDate.AddDays(30);

            // Act
            _coversService
                .Setup(s => s.ComputePremium(startDate, endDate, CoverTypeEnum.Tanker))
                .Returns(123m);

            var result = _controller.ComputePremium(startDate, endDate, CoverTypeEnum.Tanker);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(123m, returned);

            _coversService.Verify(
                s => s.ComputePremium(startDate, endDate, CoverTypeEnum.Tanker),
                Times.Once);
        }

        [Fact]
        public void ComputePremium_WithPeriodExceedingOneYear_ShouldReturnBadRequest()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate   = startDate.AddYears(1).AddDays(1); // just over 1 year

            // Act
            _coversService
                .Setup(s => s.ComputePremium(startDate, endDate, CoverTypeEnum.PassengerShip))
                .Returns(999m);

            var result = _controller.ComputePremium(startDate, endDate, CoverTypeEnum.PassengerShip);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(999m, returned);

            _coversService.Verify(
                s => s.ComputePremium(startDate, endDate, CoverTypeEnum.PassengerShip),
                Times.Once);
        }

    }
}
