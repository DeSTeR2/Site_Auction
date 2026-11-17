// Maps the user_tokens table: hashed one-time tokens looked up by user, purpose and newest first.
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Users;

public sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("user_tokens", table =>
            table.HasCheckConstraint("ck_user_tokens_purpose", "purpose IN ('email_confirm', 'password_reset')"));

        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash).IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.Purpose, token.CreatedAt })
            .IsDescending(false, false, true);
    }
}
