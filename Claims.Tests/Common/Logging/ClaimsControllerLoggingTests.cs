namespace Claims.Tests.Common.Logging;

using Claims.Common.Logging;
using Claims.Controllers;
using Claims.Domain.Entities;
using Claims.Services.ClaimsServices;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public class ClaimsControllerLoggingTests : IDisposable
{
    private readonly string _testLogFile;
    private readonly IClaimsService _claimsService;
    private readonly IValidator<Claim> _validator;
    private readonly ILogger<ClaimsController> _logger;
    private readonly ClaimsController _controller;

    public ClaimsControllerLoggingTests()
    {
        _testLogFile = Path.Combine(Path.GetTempPath(), $"claims_test_log_{Guid.NewGuid()}.txt");
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFile(_testLogFile);
            builder.SetMinimumLevel(LogLevel.Information);
        });

        _logger = loggerFactory.CreateLogger<ClaimsController>();
        _claimsService = Substitute.For<IClaimsService>();
        _validator = Substitute.For<IValidator<Claim>>();

        _controller = new ClaimsController(_logger, _claimsService, _validator);
    }

    [Fact]
    public async Task GetAsync_WritesInformationToLogFile()
    {
        // Arrange
        _claimsService.GetClaimsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Claim> { new() { Id = "claim-1", Name = "Test Claim" } });

        // Act
        var result = await _controller.GetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.True(File.Exists(_testLogFile));
        var logContent = await File.ReadAllTextAsync(_testLogFile);
        Assert.Contains("Retrieving all claims", logContent);
        Assert.Contains("Retrieved 1 claims successfully", logContent);
    }

    [Fact]
    public async Task CreateAsync_WhenValidationFails_WritesWarningToLogFile()
    {
        // Arrange
        var claim = new Claim { CoverId = "c1", DamageCost = 200_000 };
        _validator.ValidateAsync(claim, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("DamageCost", "DamageCost cannot exceed 100,000.")
            }));

        // Act
        var result = await _controller.CreateAsync(claim, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        var logContent = await File.ReadAllTextAsync(_testLogFile);
        Assert.Contains("[Warning]", logContent);
        Assert.Contains("Claim validation failed with 1 errors", logContent);
    }

    [Fact]
    public async Task DeleteAsync_WritesInformationToLogFile()
    {
        // Arrange
        _claimsService.DeleteClaimAsync("claim-1", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.DeleteAsync("claim-1", CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var logContent = await File.ReadAllTextAsync(_testLogFile);
        Assert.Contains("Attempting to delete claim with ID: claim-1", logContent);
        Assert.Contains("Successfully deleted claim with ID: claim-1", logContent);
    }

    public void Dispose()
    {
        if (File.Exists(_testLogFile))
        {
            try { File.Delete(_testLogFile); } catch { }
        }
        GC.SuppressFinalize(this);
    }
}
