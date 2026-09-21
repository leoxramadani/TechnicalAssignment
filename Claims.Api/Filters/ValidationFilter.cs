using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Claims.Api.Filters
{
    public sealed class ValidationFilter(ILogger<ValidationFilter> logger) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                    continue;

                var result = await validator.ValidateAsync(
                    new ValidationContext<object>(argument),
                    context.HttpContext.RequestAborted);

                if (result.IsValid) continue;

                logger.LogWarning(
                    "{Type} validation failed with {Count} errors: {Errors}",
                    argument.GetType().Name,
                    result.Errors.Count,
                    string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));

                context.Result = new BadRequestObjectResult(
                    new ValidationProblemDetails(result.ToDictionary())
                    {
                        Status = StatusCodes.Status400BadRequest
                    });
                return;
            }

            await next();
        }
    }
}
