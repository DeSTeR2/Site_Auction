// Chooses the IEmailSender implementation from configuration; an unknown provider stops the application at startup.
using Auction.Core.Common.Email;

namespace Auction.Infrastructure.Email;

public static class EmailSenderFactory
{
    public const string Smtp = "Smtp";
    public const string File = "File";

    public static IEmailSender Create(EmailOptions options) => options.Provider switch
    {
        Smtp => new SmtpEmailSender(options),
        File => new FileEmailSender(options),
        _ => throw new InvalidOperationException($"Unknown email provider '{options.Provider}'.")
    };
}
