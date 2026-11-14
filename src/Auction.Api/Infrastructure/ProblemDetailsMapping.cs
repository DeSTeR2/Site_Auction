// The single mapping from Result/AppError to HTTP responses in the format of design/08-api.md §2.
using System.Diagnostics;
using Auction.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Infrastructure;

public static class ProblemDetailsMapping
{
    public const string ContentType = "application/problem+json";

    private const string ErrorTypeBase = "https://auction.example/errors/";

    public static ProblemDetails ToProblemDetails(this AppError error, HttpContext httpContext)
    {
        var problem = new ProblemDetails
        {
            Type = ErrorTypeBase + error.Code,
            Title = error.Message,
            Status = error.Status
        };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["data"] = error.Data;
        problem.Extensions["errors"] = error.Errors;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        return problem;
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext httpContext)
    {
        if (!result.IsSuccess)
        {
            return result.Error!.ToObjectResult(httpContext);
        }

        return result.Value is Unit ? new NoContentResult() : new OkObjectResult(result.Value);
    }

    public static ObjectResult ToObjectResult(this AppError error, HttpContext httpContext) =>
        new(error.ToProblemDetails(httpContext))
        {
            StatusCode = error.Status,
            ContentTypes = { ContentType }
        };

    public static async Task WriteProblemAsync(this HttpContext httpContext, AppError error)
    {
        httpContext.Response.StatusCode = error.Status;
        await httpContext.Response.WriteAsJsonAsync(
            error.ToProblemDetails(httpContext),
            (System.Text.Json.JsonSerializerOptions?)null,
            ContentType,
            httpContext.RequestAborted);
    }
}
