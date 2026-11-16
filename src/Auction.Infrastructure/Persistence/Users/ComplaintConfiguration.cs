// Maps the complaints table: one new complaint per author and lot, comment required for reason "other".
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Users;

public sealed class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("complaints", table =>
        {
            table.HasCheckConstraint(
                "ck_complaints_reason",
                "reason IN ('prohibited_item', 'fraud_or_counterfeit', 'false_description', 'offensive_content', 'fake_bids', 'other')");
            table.HasCheckConstraint("ck_complaints_status", "status IN ('new', 'accepted', 'rejected')");
            table.HasCheckConstraint("ck_complaints_other_needs_comment", "reason <> 'other' OR comment IS NOT NULL");
        });

        builder.HasKey(complaint => complaint.Id);

        builder.Property(complaint => complaint.Comment).HasMaxLength(1000);

        builder.HasOne<User>().WithMany().HasForeignKey(complaint => complaint.AuthorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(complaint => complaint.ReviewedBy).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(complaint => new { complaint.AuthorId, complaint.LotId })
            .IsUnique()
            .HasFilter("status = 'new'");
    }
}
