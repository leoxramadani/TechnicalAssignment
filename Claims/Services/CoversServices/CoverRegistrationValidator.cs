namespace Claims.Services.CoversServices;

using Claims.Domain.Entities;
using FluentValidation;

public class CoverRegistrationValidator : AbstractValidator<Cover>
{
    public CoverRegistrationValidator()
    {

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("StartDate is required.")
            .Must(startDate => startDate.Date >= DateTime.UtcNow.Date)
            .WithMessage("StartDate cannot be in the past.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("EndDate is required.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("EndDate must be after StartDate. Same-day covers are not allowed.")
            .LessThanOrEqualTo(x => x.StartDate.AddYears(1))
            .WithMessage("The total insurance period cannot exceed 1 year.");

    }
}
