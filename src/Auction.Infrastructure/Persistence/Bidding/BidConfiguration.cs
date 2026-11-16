// Maps the bids table; UNIQUE (lot_id, amount) is the database-level safety net for the bidding guarantees (design/04 §3).
using Auction.Core.Bidding.Domain;
using Auction.Core.Lots.Domain;
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Bidding;

public sealed class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("bids", table => table.HasCheckConstraint("ck_bids_amount", "amount BETWEEN 1 AND 100000000"));

        builder.HasKey(bid => bid.Id);

        builder.HasOne<Lot>().WithMany().HasForeignKey(bid => bid.LotId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(bid => bid.BidderId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bid => new { bid.LotId, bid.Amount }).IsUnique();
        builder.HasIndex(bid => new { bid.LotId, bid.PlacedAt }).IsDescending(false, true);
        builder.HasIndex(bid => new { bid.BidderId, bid.LotId });
    }
}
