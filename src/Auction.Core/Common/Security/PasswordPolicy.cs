// Password rules of project.md §6.1 in one place for registration, change and reset.
namespace Auction.Core.Common.Security;

public static class PasswordPolicy
{
    public const int MinLength = 8;

    public const string Message = "Пароль: від 8 символів, щонайменше одна літера і одна цифра";

    public static bool IsValid(string? password) =>
        password is { Length: >= MinLength }
        && password.Any(char.IsLetter)
        && password.Any(char.IsDigit);
}
