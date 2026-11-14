// Secure token generation and hashing, and the password policy of project.md §6.1.
using Auction.Core.Common.Security;

namespace Auction.UnitTests.Platform;

public class SecurityPrimitivesTests
{
    [Fact]
    public void Generated_tokens_are_unique_and_hash_to_sha256()
    {
        var first = SecureToken.Generate();
        var second = SecureToken.Generate();

        Assert.NotEqual(first, second);
        Assert.Equal(43, first.Length);
        Assert.Equal(32, SecureToken.Hash(first)!.Length);
        Assert.Equal(SecureToken.Hash(first), SecureToken.Hash(first));
        Assert.NotEqual(SecureToken.Hash(first), SecureToken.Hash(second));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("not a token!")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void Token_of_wrong_shape_has_no_hash(string? token)
    {
        Assert.Null(SecureToken.Hash(token));
    }

    [Theory]
    [InlineData("password1", true)]
    [InlineData("пароль123", true)]
    [InlineData("12345678", false)]
    [InlineData("password", false)]
    [InlineData("pass1", false)]
    [InlineData(null, false)]
    public void Password_policy(string? password, bool expected)
    {
        Assert.Equal(expected, PasswordPolicy.IsValid(password));
    }
}
