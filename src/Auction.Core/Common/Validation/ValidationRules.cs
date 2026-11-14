// Shared FluentValidation rules so every module validates passwords the same way.
using Auction.Core.Common.Security;
using FluentValidation;

namespace Auction.Core.Common.Validation;

public static class ValidationRules
{
    public static IRuleBuilderOptions<T, string?> Password<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(PasswordPolicy.IsValid).WithMessage(PasswordPolicy.Message);
}
