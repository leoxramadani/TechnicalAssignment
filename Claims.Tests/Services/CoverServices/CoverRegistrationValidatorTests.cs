namespace Claims.Tests.Services.CoverServices;

using Claims.Domain.Entities;
using Claims.Services.CoversServices;
using FluentValidation.TestHelper;
using Xunit;

public class CoverRegistrationValidatorTests
{
    private readonly CoverRegistrationValidator _validator;

    public CoverRegistrationValidatorTests()
    {
        _validator = new CoverRegistrationValidator();
    }

    [Fact]
    public void Validate_WhenStartDateIsInThePast_ShouldHaveValidationError()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };

        // Act
        var result = _validator.TestValidate(cover);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("StartDate cannot be in the past.");
    }

    [Fact]
    public void Validate_WhenStartDateIsTodayOrFuture_ShouldNotHaveValidationErrorForStartDate()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };

        // Act
        var result = _validator.TestValidate(cover);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_WhenEndDateIsBeforeStartDate_ShouldHaveValidationError()
    {
        // Arrange
        var cover = new Cover
        {
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(5)
        };

        // Act
        var result = _validator.TestValidate(cover);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage("EndDate must be greater than or equal to StartDate.");
    }

    [Fact]
    public void Validate_WhenInsurancePeriodExceedsOneYear_ShouldHaveValidationError()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var cover = new Cover
        {
            StartDate = startDate,
            EndDate = startDate.AddYears(1).AddDays(1)
        };

        // Act
        var result = _validator.TestValidate(cover);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage("The total insurance period cannot exceed 1 year.");
    }

    [Fact]
    public void Validate_WhenCoverIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var cover = new Cover
        {
            StartDate = startDate,
            EndDate = startDate.AddYears(1) // exactly 1 year
        };

        // Act
        var result = _validator.TestValidate(cover);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
