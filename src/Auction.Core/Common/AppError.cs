// Application error with a stable code; the API maps it to ProblemDetails.
namespace Auction.Core.Common;

public sealed record AppError(string Code, int Status, string Message, object? Data = null);
