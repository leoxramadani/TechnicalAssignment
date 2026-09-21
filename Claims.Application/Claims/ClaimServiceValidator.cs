namespace Claims.Application.Claims;


using FluentValidation;
using global::Claims.Application.Covers;
using global::Claims.Domain.Entities;

public class ClaimServiceValidator : AbstractValidator<Claim>
{
    private readonly ICoversService _coversService;

    public ClaimServiceValidator(ICoversService coversService)
    {
        _coversService = coversService;

        RuleFor(x => x.DamageCost)
            .GreaterThan(0)
            .WithMessage("DamageCost must be greater than 0.")
            .LessThanOrEqualTo(100_000)
            .WithMessage("DamageCost cannot exceed 100,000.");

        RuleFor(x => x.CoverId)
            .NotEmpty()
            .WithMessage("CoverId is required.");

        RuleFor(x => x.Created)
            .NotEmpty()
            .WithMessage("Created date is required.");

        RuleFor(x => x)
            .CustomAsync(async (claim, context, cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(claim.CoverId))
                {
                    return;
                }

                var cover = await _coversService.GetCoverByIdAsync(claim.CoverId, cancellationToken);
                if (cover is null)
                {
                    context.AddFailure(nameof(claim.CoverId), "The related Cover does not exist.");
                    return;
                }

                if (claim.Created < cover.StartDate || claim.Created > cover.EndDate)
                {
                    context.AddFailure(nameof(claim.Created), "Created date must be within the period of the related Cover.");
                }
            });
    }
}
