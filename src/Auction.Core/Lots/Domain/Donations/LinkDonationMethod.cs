// Https donation link with normalization used to look it up among approved links.
using Auction.Core.Common;

namespace Auction.Core.Lots.Domain.Donations;

public sealed class LinkDonationMethod : IDonationMethod
{
    private static readonly string[] TrackingParameters = ["fbclid", "gclid"];

    private LinkDonationMethod(string display, Uri donateUri, string normalizedKey)
    {
        Display = display;
        DonateUri = donateUri;
        NormalizedKey = normalizedKey;
    }

    public DonationMethodKind Kind => DonationMethodKind.Link;

    public string Display { get; }

    public Uri DonateUri { get; }

    public string NormalizedKey { get; }

    public static Result<IDonationMethod> Create(string? rawValue)
    {
        var trimmed = rawValue?.Trim();
        if (string.IsNullOrEmpty(trimmed)
            || trimmed.Length > 500
            || !Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps)
        {
            return AppErrors.Donation.LinkInvalid;
        }

        return Result<IDonationMethod>.Success(new LinkDonationMethod(trimmed, uri, Normalize(uri)));
    }

    private static string Normalize(Uri uri)
    {
        var host = uri.Host.ToLowerInvariant();
        if (host.StartsWith("www.", StringComparison.Ordinal))
        {
            host = host[4..];
        }

        var port = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
        var path = uri.AbsolutePath.TrimEnd('/');
        var query = string.Join('&', uri.Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(pair => !IsTrackingParameter(pair)));

        return query.Length == 0
            ? $"https://{host}{port}{path}"
            : $"https://{host}{port}{path}?{query}";
    }

    private static bool IsTrackingParameter(string pair)
    {
        var name = pair.Split('=')[0].ToLowerInvariant();
        return name.StartsWith("utm_", StringComparison.Ordinal) || TrackingParameters.Contains(name);
    }
}
