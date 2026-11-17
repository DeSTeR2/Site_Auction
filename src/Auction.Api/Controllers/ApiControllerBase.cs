// Base for thin controllers: DTO → service → Result → 200/201/204 or ProblemDetails.
using Auction.Api.Infrastructure;
using Auction.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result) => result.ToActionResult(HttpContext);

    protected IActionResult CreatedFromResult<T>(Result<T> result, string location) =>
        result.IsSuccess ? Created(location, result.Value) : result.ToActionResult(HttpContext);
}
