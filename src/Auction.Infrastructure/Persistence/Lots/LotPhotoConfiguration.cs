// Maps lot_photos: photos are removed together with their lot and read in (lot_id, position) order.
using Auction.Core.Lots.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Lots;

public sealed class LotPhotoConfiguration : IEntityTypeConfiguration<LotPhoto>
{
    public void Configure(EntityTypeBuilder<LotPhoto> builder)
    {
        builder.ToTable("lot_photos");

        builder.HasKey(photo => photo.Id);

        builder.Property(photo => photo.LargeKey).IsRequired();
        builder.Property(photo => photo.ThumbKey).IsRequired();

        builder.HasOne<Lot>().WithMany().HasForeignKey(photo => photo.LotId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(photo => new { photo.LotId, photo.Position });
    }
}
