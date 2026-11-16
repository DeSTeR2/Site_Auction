// Maps donation_links: one row per normalized URL with its review status and reviewer.
using Auction.Core.Lots.Domain;
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Lots;

public sealed class DonationLinkConfiguration : IEntityTypeConfiguration<DonationLink>
{
    public void Configure(EntityTypeBuilder<DonationLink> builder)
    {
        builder.ToTable("donation_links", table =>
            table.HasCheckConstraint("ck_donation_links_status", "status IN ('pending', 'approved', 'rejected', 'revoked')"));

        builder.HasKey(link => link.Id);

        builder.Property(link => link.NormalizedUrl).IsRequired();
        builder.Property(link => link.OriginalUrl).IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(link => link.SubmittedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(link => link.ReviewedBy).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(link => link.NormalizedUrl).IsUnique();
        builder.HasIndex(link => new { link.Status, link.CreatedAt });
    }
}
