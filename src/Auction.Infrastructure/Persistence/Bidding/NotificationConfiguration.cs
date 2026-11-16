// Maps notifications with their email outbox state, the pending-email partial index and the 90-day purge index.
using Auction.Core.Bidding.Domain;
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Bidding;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", table =>
        {
            table.HasCheckConstraint(
                "ck_notifications_type",
                "type IN ('outbid', 'you_won', 'you_lost', 'lot_sold', 'lot_unsold', 'new_bid_on_your_lot', 'favorite_ending', " +
                "'lot_hidden', 'lot_restored', 'complaint_reviewed', 'account_blocked', 'lot_approved', 'lot_rejected')");
            table.HasCheckConstraint("ck_notifications_email_status", "email_status IN ('none', 'pending', 'sent', 'failed')");
            table.HasCheckConstraint("ck_notifications_email_attempts", "email_attempts BETWEEN 0 AND 3");
        });

        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.Text).IsRequired();
        builder.Property(notification => notification.Link).IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(notification => notification.RecipientId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(notification => new { notification.RecipientId, notification.IsRead, notification.CreatedAt })
            .IsDescending(false, false, true);
        builder.HasIndex(notification => notification.EmailNextAttemptAt).HasFilter("email_status = 'pending'");
        builder.HasIndex(notification => notification.CreatedAt);
    }
}
