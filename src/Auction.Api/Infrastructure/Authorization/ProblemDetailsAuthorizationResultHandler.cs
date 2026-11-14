// Answers failed authorization with stable codes: guest → auth.required, no rights → access.forbidden.
using Auction.Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Auction.Api.Infrastructure.Authorization;

public sealed class ProblemDetailsAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            await context.WriteProblemAsync(AppErrors.Auth.Required);
            return;
        }

        if (authorizeResult.Forbidden)
        {
            var error = FailedOnEmailConfirmation(authorizeResult)
                ? AppErrors.Access.EmailNotConfirmed
                : AppErrors.Access.Forbidden;
            await context.WriteProblemAsync(error);
            return;
        }

        await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private static bool FailedOnEmailConfirmation(PolicyAuthorizationResult result) =>
        result.AuthorizationFailure?.FailedRequirements
            .OfType<ClaimsAuthorizationRequirement>()
            .Any(r => r.ClaimType == AuctionClaimTypes.EmailConfirmed) == true;
}
