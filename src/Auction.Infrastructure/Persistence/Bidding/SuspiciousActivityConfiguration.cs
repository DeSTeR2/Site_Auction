// Maps suspicious_activities with a jsonb details column and filters by type and user.
using Auction.Core.Bidding.Domain;
using Auction.Core.Lots.Domain;
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Bidding;

public sealed class SuspiciousActivityConfiguration : IEntityTypeConfiguration<SuspiciousActivity>
{
    public void Configure(EntityTypeBuilder<SuspiciousActivity> builder)
    {
        builder.ToTable("suspicious_activities", table =>
            table.HasCheckConstraint(
                "ck_suspicious_activities_type",
                "type IN ('own_lot_bid', 'rate_limit', 'new_account_bid', 'login_failures')"));

        builder.HasKey(activity => activity.Id);

        builder.Property(activity => activity.Details).HasColumnType("jsonb").IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(activity => activity.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Lot>().WithMany().HasForeignKey(activity => activity.LotId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(activity => new { activity.Type, activity.CreatedAt });
        builder.HasIndex(activity => activity.UserId);
    }
}
