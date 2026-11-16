// Maps the lots table: value checks, bidding columns owned by Bidding, foreign keys and search indexes.
using Auction.Core.Lots.Domain;
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Lots;

public sealed class LotConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.ToTable("lots", table =>
        {
            table.HasCheckConstraint("ck_lots_status", "status IN ('draft', 'pending_review', 'active', 'sold', 'unsold', 'withdrawn')");
            table.HasCheckConstraint("ck_lots_condition", "condition IN ('new', 'like_new', 'used', 'needs_repair')");
            table.HasCheckConstraint("ck_lots_start_price", "start_price BETWEEN 1 AND 10000000");
            table.HasCheckConstraint("ck_lots_bid_step", "bid_step BETWEEN 1 AND 1000000");
            table.HasCheckConstraint("ck_lots_title_length", "char_length(title) BETWEEN 5 AND 100");
            table.HasCheckConstraint("ck_lots_description_length", "char_length(description) BETWEEN 20 AND 5000");
            table.HasCheckConstraint("ck_lots_duration", "duration_minutes > 0");
            table.HasCheckConstraint("ck_lots_current_price", "current_price > 0");
            table.HasCheckConstraint("ck_lots_bid_counts", "bid_count >= 0 AND bidder_count >= 0 AND bidder_count <= bid_count");
            table.HasCheckConstraint("ck_lots_active_has_times", "status <> 'active' OR (started_at IS NOT NULL AND ends_at IS NOT NULL)");
            table.HasCheckConstraint("ck_lots_final_price_when_closed", "final_price IS NULL OR status = 'sold' OR status = 'unsold'");
        });

        builder.HasKey(lot => lot.Id);

        builder.Property(lot => lot.Title).HasMaxLength(100).IsRequired();
        builder.Property(lot => lot.Description).HasMaxLength(5000).IsRequired();
        builder.Property(lot => lot.DonationUrl).IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(lot => lot.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>().WithMany().HasForeignKey(lot => lot.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<DonationLink>().WithMany().HasForeignKey(lot => lot.DonationLinkId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(lot => lot.LeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(lot => lot.WinnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Lot>().WithMany().HasForeignKey(lot => lot.SourceLotId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(lot => new { lot.Status, lot.IsHidden, lot.EndsAt });
        builder.HasIndex(lot => new { lot.CategoryId, lot.Status, lot.IsHidden });
        builder.HasIndex(lot => new { lot.SellerId, lot.Status });
        builder.HasIndex(lot => new { lot.Status, lot.ClosedAt }).IsDescending(false, true);
        builder.HasIndex(lot => lot.Title).HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(lot => lot.Description).HasMethod("gin").HasOperators("gin_trgm_ops");
    }
}
