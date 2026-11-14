// 256-bit random tokens for sessions and email links; only their SHA-256 hash is ever stored.
using System.Buffers.Text;
using System.Security.Cryptography;

namespace Auction.Core.Common.Security;

public static class SecureToken
{
    public const int SizeInBytes = 32;

    public static string Generate() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(SizeInBytes));

    public static byte[]? Hash(string? token)
    {
        if (string.IsNullOrEmpty(token)
            || !Base64Url.IsValid(token, out var decodedLength)
            || decodedLength != SizeInBytes)
        {
            return null;
        }

        Span<byte> raw = stackalloc byte[SizeInBytes];
        if (!Base64Url.TryDecodeFromChars(token, raw, out var written) || written != SizeInBytes)
        {
            return null;
        }

        return SHA256.HashData(raw);
    }
}
