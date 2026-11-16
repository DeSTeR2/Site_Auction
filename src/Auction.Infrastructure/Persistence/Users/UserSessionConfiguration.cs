// Maps the sessions table: unique token hash and a cascade delete with the user.
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Users;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(session => session.Id);

        builder.Property(session => session.TokenHash).IsRequired();
        builder.Property(session => session.UserAgent).HasMaxLength(UserSession.MaxUserAgentLength);

        builder.HasOne<User>().WithMany().HasForeignKey(session => session.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(session => session.TokenHash).IsUnique();
        builder.HasIndex(session => session.UserId);
    }
}
