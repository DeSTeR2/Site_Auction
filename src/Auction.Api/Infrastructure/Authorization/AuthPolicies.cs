// Controller-level access policies from design/07-auth-security.md §4.
namespace Auction.Api.Infrastructure.Authorization;

public static class AuthPolicies
{
    public const string Authenticated = nameof(Authenticated);
    public const string EmailConfirmed = nameof(EmailConfirmed);
    public const string UserOnly = nameof(UserOnly);
    public const string Admin = nameof(Admin);
}
