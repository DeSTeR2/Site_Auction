// Maps the users table: case-insensitive email and nickname, phone and length checks.
using Auction.Core.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Users;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("ck_users_nickname_length", "char_length(nickname) BETWEEN 3 AND 30");
            table.HasCheckConstraint("ck_users_phone_format", "phone ~ '^\\+380\\d{9}$'");
            table.HasCheckConstraint("ck_users_role", "role IN ('user', 'admin')");
            table.HasCheckConstraint("ck_users_status", "status IN ('active', 'blocked')");
        });

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email).HasColumnType("citext").IsRequired();
        builder.Property(user => user.PasswordHash).IsRequired();
        builder.Property(user => user.Nickname).HasColumnType("citext").IsRequired();
        builder.Property(user => user.City).HasMaxLength(50);

        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.Nickname).IsUnique();
    }
}
