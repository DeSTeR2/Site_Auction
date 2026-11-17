// A validated way to donate for a lot; v1.0 has only https links (design/14-charity.md §2).
namespace Auction.Core.Lots.Domain.Donations;

public enum DonationMethodKind
{
    Link
}

public interface IDonationMethod
{
    DonationMethodKind Kind { get; }

    string Display { get; }

    Uri DonateUri { get; }

    string NormalizedKey { get; }
}
