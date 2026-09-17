namespace Claims.Tests.Services.ClaimServices;

using Claims.Domain.Entities;
using Claims.Services.ClaimsServices;
using Claims.Services.CoversServices;
using FluentValidation.TestHelper;
using NSubstitute;
using Xunit;

public class ClaimRegistrationValidatorTests
{
    private readonly ICoversService _coversService;
    private readonly ClaimServiceValidator _validator;

    public ClaimRegistrationValidatorTests()
    {
        _coversService = Substitute.For<ICoversService>();
        _validator = new ClaimServiceValidator(_coversService);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task ValidateAsync_WhenDamageCostIsZeroOrNegative_ShouldHaveValidationError(decimal damageCost)
    {
        // Arrange
        var claim = new Claim
        {
            CoverId = "cover-1",
            Created = DateTime.UtcNow,
            DamageCost = damageCost
        };

        // Act
        var result = await _validator.TestValidateAsync(claim);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DamageCost);
    }

    [Fact]
    public async Task ValidateAsync_WhenDamageCostExceeds100000_ShouldHaveValidationError()
    {
        // Arrange
        var claim = new Claim
        {
            CoverId = "cover-1",
            Created = DateTime.UtcNow,
            DamageCost = 100_001m
        };

        // Act
        var result = await _validator.TestValidateAsync(claim);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DamageCost)
            .WithErrorMessage("DamageCost cannot exceed 100,000.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ValidateAsync_WhenCoverIdIsEmpty_ShouldHaveValidationError(string? coverId)
    {
        // Arrange
        var claim = new Claim
        {
            CoverId = coverId!,
            Created = DateTime.UtcNow,
            DamageCost = 500m
        };

        // Act
        var result = await _validator.TestValidateAsync(claim);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoverId);
    }

    [Fact]
    public async Task ValidateAsync_WhenCoverDoesNotExist_ShouldHaveValidationErrorForCoverIdOnly()
    {
        // Arrange
        const string coverId = "non-existent-cover";
        _coversService.GetCoverByIdAsync(coverId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Cover?>(null));

        var claim = new Claim
        {
            CoverId = coverId,
            Created = DateTime.UtcNow,
            DamageCost = 500m
        };

        // Act
        var result = await _validator.TestValidateAsync(claim);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoverId)
            .WithErrorMessage("The related Cover does not exist.");
        result.ShouldNotHaveValidationErrorFor(x => x.Created);
    }

    [Fact]
    public async Task ValidateAsync_WhenCreatedIsBeforeCoverStartDate_ShouldHaveValidationErrorForCreated()
    {
        // Arrange
        const string coverId = "cover-1";
        var cover = new Cover
        {
            Id = coverId,
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(40)
        };
        _coversService.GetCoverByIdAsync(coverId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Cover?>(cover));

        var claim = new Claim
        {
            CoverId = coverId,
            Created = DateTime.UtcNow.Date.AddDays(5), // before start date
            DamageCost = 500m
        };

        // Act
        var result = await _validator.TestValidateAsync(claim);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Created)
            .WithErrorMessage("Created date must be within the period of the related Cover.");
    }

    [Fact]
    public async Task ValidateAsync_WhenCreatedIsAfterCoverEndDate_ShouldHaveValidationErrorForCreated()
    {
        // Arrange
        const string coverId = "cover-1";
        var cover = new Cover
        {
            Id = coverId,
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(40)
        };
        _coversService.GetCoverByIdAsync(coverId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Cover?>(cover));

        var claim = new Claim
        {
            CoverId = coverId,
            Created = DateTime.UtcNow.Date.AddDays(45), // after end date
            DamageCost = 500m
        };

        // Act
        var result = await _validator.TestValidateAsync(claim);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Created)
            .WithErrorMessage("Created date must be within the period of the related Cover.");
    }

    [Fact]
    public async Task ValidateAsync_WhenClaimIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        const string coverId = "cover-1";
        var cover = new Cover
        {
            Id = coverId,
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };
        _coversService.GetCoverByIdAsync(coverId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Cover?>(cover));

        var claim = new Claim
        {
            CoverId = coverId,
            Created = DateTime.UtcNow.Date.AddDays(15),
            DamageCost = 50_000m
        };

        // Act
        var result = await _validator.TestValidateAsync(claim);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
