// Picks the donation method implementation by kind; new kinds (jar, IBAN, payments) register one line here.
using Auction.Core.Common;

namespace Auction.Core.Lots.Domain.Donations;

public interface IDonationMethodFactory
{
    Result<IDonationMethod> Create(DonationMethodKind kind, string rawValue);
}

public sealed class DonationMethodFactory : IDonationMethodFactory
{
    private readonly Dictionary<DonationMethodKind, Func<string, Result<IDonationMethod>>> creators = new()
    {
        [DonationMethodKind.Link] = LinkDonationMethod.Create,
    };

    public Result<IDonationMethod> Create(DonationMethodKind kind, string rawValue) =>
        creators.TryGetValue(kind, out var create)
            ? create(rawValue)
            : throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported donation method.");
}
