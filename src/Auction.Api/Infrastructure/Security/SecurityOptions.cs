// Request protection settings from design/07-auth-security.md §5, bound to "Security".
using System.ComponentModel.DataAnnotations;

namespace Auction.Api.Infrastructure.Security;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public string[] AllowedOrigins { get; init; } = [];

    [Range(1, 10_000)]
    public int AuthRequestsPerMinute { get; init; } = 20;
}
