// Maps favorites with the composite key (user_id, lot_id) and an index by lot for reminders and statistics.
using Auction.Core.Lots.Domain;
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Lots;

public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("favorites");

        builder.HasKey(favorite => new { favorite.UserId, favorite.LotId });

        builder.HasOne<User>().WithMany().HasForeignKey(favorite => favorite.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Lot>().WithMany().HasForeignKey(favorite => favorite.LotId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(favorite => favorite.LotId);
    }
}
