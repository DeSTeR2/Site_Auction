// Outgoing email channel used by notification delivery and account flows.
namespace Auction.Core.Common.Email;

public sealed record EmailMessage(string To, string Subject, string HtmlBody);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct);
}
