// Sends email through an SMTP provider with MailKit.
using Auction.Core.Common.Email;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Auction.Infrastructure.Email;

public sealed class SmtpEmailSender(EmailOptions options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var mime = MimeMessageMapper.ToMime(message, options);

        using var client = new SmtpClient();
        await client.ConnectAsync(options.Smtp.Host, options.Smtp.Port, SecureSocketOptions.StartTlsWhenAvailable, ct);
        if (!string.IsNullOrEmpty(options.Smtp.UserName))
        {
            await client.AuthenticateAsync(options.Smtp.UserName, options.Smtp.Password ?? string.Empty, ct);
        }

        await client.SendAsync(new SendRequest(mime), ct);
        await client.DisconnectAsync(true, ct);
    }
}
