// Validates action arguments with their registered FluentValidation validators before the controller runs.
using Auction.Core.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auction.Api.Infrastructure.Validation;

public sealed class FluentValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var argument in context.ActionArguments.Values.OfType<object>())
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var failure in result.Errors)
            {
                var key = FieldNames.ToKey(failure.PropertyName);
                if (!errors.TryGetValue(key, out var messages))
                {
                    errors[key] = messages = [];
                }

                messages.Add(failure.ErrorMessage);
            }
        }

        if (errors.Count > 0)
        {
            var fieldErrors = errors.ToDictionary(e => e.Key, e => e.Value.ToArray());
            context.Result = AppErrors.ValidationFailed.WithErrors(fieldErrors).ToObjectResult(context.HttpContext);
            return;
        }

        await next();
    }
}
