// Converts an EmailMessage into a MIME message shared by the SMTP and file senders.
using Auction.Core.Common.Email;
using MimeKit;

namespace Auction.Infrastructure.Email;

internal static class MimeMessageMapper
{
    public static MimeMessage ToMime(EmailMessage message, EmailOptions options)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(options.FromName, options.From));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();
        return mime;
    }
}
