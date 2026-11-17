// The single mapping from Result/AppError to HTTP responses in the format of design/08-api.md §2.
using System.Diagnostics;
using Auction.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Infrastructure;

public static class ProblemDetailsMapping
{
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
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        return problem;
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext httpContext)
    {
        if (!result.IsSuccess)
        {
            return new ObjectResult(result.Error!.ToProblemDetails(httpContext))
            {
                StatusCode = result.Error!.Status,
                ContentTypes = { "application/problem+json" }
            };
        }

        return result.Value is Unit ? new NoContentResult() : new OkObjectResult(result.Value);
    }
}
