// What a one-time user token is issued for (design/07-auth-security.md §2).
namespace Auction.Core.Common.Enums;

public enum TokenPurpose
{
    EmailConfirm,
    PasswordReset
}
