// REST conventions of design/08-api.md §1: camelCase JSON, snake_case enums, validation.failed for bad input.
using System.Text.Json;
using System.Text.Json.Serialization;
using Auction.Api.Infrastructure.Validation;
using Auction.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Infrastructure;

public static class ApiSetup
{
    private const string InvalidValueMessage = "Некоректне значення";

    public static IServiceCollection AddAuctionApi(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<FluentValidationFilter>())
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)));

        services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = context =>
            {
                var fieldErrors = context.ModelState
                    .Where(entry => entry.Value is { Errors.Count: > 0 })
                    .ToDictionary(
                        entry => FieldNames.ToKey(entry.Key.TrimStart('$', '.')),
                        entry => entry.Value!.Errors
                            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? InvalidValueMessage : e.ErrorMessage)
                            .ToArray());

                return AppErrors.ValidationFailed.WithErrors(fieldErrors).ToObjectResult(context.HttpContext);
            });

        services.AddProblemDetails();
        services.AddExceptionHandler<ServerErrorExceptionHandler>();
        return services;
    }
}
