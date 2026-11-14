// Turns any unhandled exception into a 500 server.error response without leaking details.
using Auction.Core.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace Auction.Api.Infrastructure;

public sealed class ServerErrorExceptionHandler(ILogger<ServerErrorExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        await httpContext.WriteProblemAsync(AppErrors.ServerError);
        return true;
    }
}
