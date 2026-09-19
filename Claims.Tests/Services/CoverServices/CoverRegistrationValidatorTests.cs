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
            .WithErrorMessage("EndDate must be after StartDate. Same-day covers are not allowed.");
    }

    [Fact]
    public void Validate_WhenEndDateEqualToStartDate_ShouldHaveValidationError()
    {
        // Arrange – same-day cover produces a zero-day period and £0 premium
        var today = DateTime.UtcNow.Date.AddDays(1);
        var cover = new Cover
        {
            StartDate = today,
            EndDate = today
        };

        // Act
        var result = _validator.TestValidate(cover);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage("EndDate must be after StartDate. Same-day covers are not allowed.");
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
        // Arrange – endDate must be strictly after startDate
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var cover = new Cover
        {
            StartDate = startDate,
            EndDate = startDate.AddDays(1) // minimum valid cover: 1 day
        };

        // Act
        var result = _validator.TestValidate(cover);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
