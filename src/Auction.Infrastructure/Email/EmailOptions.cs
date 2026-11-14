// Email settings bound to the "Email" configuration section; the SMTP password comes from the environment.
using System.ComponentModel.DataAnnotations;

namespace Auction.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    public string Provider { get; init; } = EmailSenderFactory.Smtp;

    [Required]
    [EmailAddress]
    public string From { get; init; } = "no-reply@auction.local";

    public string FromName { get; init; } = "Благодійний аукціон";

    public string FileDirectory { get; init; } = "mail-drop";

    public SmtpOptions Smtp { get; init; } = new();
}

public sealed class SmtpOptions
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 587;

    public string? UserName { get; init; }

    public string? Password { get; init; }
}
