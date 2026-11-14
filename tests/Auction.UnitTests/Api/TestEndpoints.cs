// Test-only controllers that exercise policies, validation, errors and sign-in through the real pipeline.
using Auction.Api.Controllers;
using Auction.Api.Infrastructure.Authentication;
using Auction.Api.Infrastructure.Authorization;
using Auction.Core.Common;
using Auction.Core.Platform.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auction.UnitTests.Api;

public sealed record EchoRequest(string? Title, int Amount);

public sealed class EchoRequestValidator : AbstractValidator<EchoRequest>
{
    public EchoRequestValidator()
    {
        RuleFor(r => r.Title).NotEmpty().WithMessage("Назва: від 5 до 100 символів").Length(5, 100).WithMessage("Назва: від 5 до 100 символів");
        RuleFor(r => r.Amount).GreaterThan(0).WithMessage("Сума має бути більшою за 0");
    }
}

[Route("api/test")]
public sealed class TestEndpointsController(ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("public")]
    public IActionResult Public() => Ok(new { userId = currentUser.UserId });

    [HttpPost("public")]
    public IActionResult PublicPost() => NoContent();

    [Authorize(Policy = AuthPolicies.Authenticated)]
    [HttpGet("authenticated")]
    public IActionResult Authenticated() => Ok(new { userId = currentUser.UserId, isAdmin = currentUser.IsAdmin });

    [Authorize(Policy = AuthPolicies.UserOnly)]
    [HttpGet("user-only")]
    public IActionResult UserOnly() => NoContent();

    [Authorize(Policy = AuthPolicies.Admin)]
    [HttpGet("admin")]
    public IActionResult Admin() => NoContent();

    [Authorize(Policy = AuthPolicies.EmailConfirmed)]
    [HttpGet("email-confirmed")]
    public IActionResult EmailConfirmed() => NoContent();

    [HttpPost("echo")]
    public IActionResult Echo(EchoRequest request) => Ok(request);

    [HttpGet("conflict")]
    public IActionResult HasBidsConflict() => FromResult(Result<Unit>.Failure(AppErrors.Lot.HasBids));

    [HttpGet("throw")]
    public IActionResult Throw() => throw new InvalidOperationException("secret internal detail");

    [HttpPost("sign-in/{userId:long}")]
    public async Task<IActionResult> SignIn(long userId, [FromServices] ISessionSignIn signIn)
    {
        await signIn.SignInAsync(HttpContext, userId, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost("sign-out")]
    public async Task<IActionResult> SignOutSession([FromServices] ISessionSignIn signIn)
    {
        await signIn.SignOutAsync(HttpContext, HttpContext.RequestAborted);
        return NoContent();
    }
}

[Route("api/auth")]
public sealed class AuthPingController : ApiControllerBase
{
    [HttpPost("ping")]
    public IActionResult Ping() => NoContent();
}
