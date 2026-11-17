// Development sender that writes every email as an .eml file into a local folder.
using Auction.Core.Common.Email;

namespace Auction.Infrastructure.Email;

public sealed class FileEmailSender(EmailOptions options, TimeProvider clock) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        Directory.CreateDirectory(options.FileDirectory);

        var fileName = $"{clock.GetUtcNow():yyyyMMdd-HHmmssfff}-{Guid.NewGuid():N}.eml";
        var path = Path.Combine(options.FileDirectory, fileName);

        var mime = MimeMessageMapper.ToMime(message, options);
        await mime.WriteToAsync(path, ct);
    }
}
