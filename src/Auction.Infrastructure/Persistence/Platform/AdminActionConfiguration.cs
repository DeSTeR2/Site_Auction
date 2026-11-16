// Maps the admin_actions table; the migration also forbids UPDATE and DELETE on it.
using Auction.Core.Platform.Domain;
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Platform;

public sealed class AdminActionConfiguration : IEntityTypeConfiguration<AdminAction>
{
    public void Configure(EntityTypeBuilder<AdminAction> builder)
    {
        builder.ToTable("admin_actions", table =>
            table.HasCheckConstraint(
                "ck_admin_actions_action",
                "action IN ('hide_lot', 'restore_lot', 'accept_complaint', 'reject_complaint', 'block_user', 'unblock_user', 'approve_link', 'reject_link', 'revoke_link')"));

        builder.HasKey(action => action.Id);

        builder.Property(action => action.TargetType).IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(action => action.AdminId).OnDelete(DeleteBehavior.Restrict);
    }
}
