// Who is making the current request; resolved once per request by the API layer.
using Auction.Core.Common.Enums;

namespace Auction.Core.Platform.Contracts;

public interface ICurrentUser
{
    long? UserId { get; }

    UserRole Role { get; }

    bool IsAuthenticated { get; }

    bool IsEmailConfirmed { get; }

    bool IsAdmin { get; }

    string VisitorKey { get; }
}
