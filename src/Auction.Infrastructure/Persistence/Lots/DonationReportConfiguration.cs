// Maps donation_reports: at most one report per lot, keyed by lot_id.
using Auction.Core.Lots.Domain;
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Lots;

public sealed class DonationReportConfiguration : IEntityTypeConfiguration<DonationReport>
{
    public void Configure(EntityTypeBuilder<DonationReport> builder)
    {
        builder.ToTable("donation_reports", table => table.HasCheckConstraint("ck_donation_reports_amount", "amount > 0"));

        builder.HasKey(report => report.LotId);
        builder.Property(report => report.LotId).ValueGeneratedNever();

        builder.HasOne<Lot>().WithOne().HasForeignKey<DonationReport>(report => report.LotId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(report => report.WinnerId).OnDelete(DeleteBehavior.Restrict);
    }
}
