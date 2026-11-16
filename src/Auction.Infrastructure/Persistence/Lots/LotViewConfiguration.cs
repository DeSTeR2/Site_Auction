// Maps lot_views with the key (lot_id, visitor_key, view_date) so one visitor counts once per day.
using Auction.Core.Lots.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Lots;

public sealed class LotViewConfiguration : IEntityTypeConfiguration<LotView>
{
    public void Configure(EntityTypeBuilder<LotView> builder)
    {
        builder.ToTable("lot_views");

        builder.HasKey(view => new { view.LotId, view.VisitorKey, view.ViewDate });

        builder.Property(view => view.VisitorKey).IsRequired();

        builder.HasOne<Lot>().WithMany().HasForeignKey(view => view.LotId).OnDelete(DeleteBehavior.Cascade);
    }
}
