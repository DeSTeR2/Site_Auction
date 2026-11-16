// User account: credentials, profile, role and status (project.md §6.1); blocking raises UserBlocked.
using Auction.Core.Common.Enums;
using Auction.Core.Common.Events;
using Auction.Core.Users.Contracts;

namespace Auction.Core.Users.Domain;

public sealed class User : Entity
{
    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Nickname = string.Empty;
    }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTimeOffset? EmailConfirmedAt { get; private set; }

    public string Nickname { get; private set; }

    public string? Phone { get; private set; }

    public string? City { get; private set; }

    public string? AvatarKey { get; private set; }

    public UserRole Role { get; private set; }

    public UserStatus Status { get; private set; }

    public bool EmailNotifications { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsEmailConfirmed => EmailConfirmedAt is not null;

    public bool IsBlocked => Status == UserStatus.Blocked;

    public static User Register(string email, string nickname, string passwordHash, DateTimeOffset now) => new()
    {
        Email = email,
        Nickname = nickname,
        PasswordHash = passwordHash,
        Role = UserRole.User,
        Status = UserStatus.Active,
        EmailNotifications = true,
        CreatedAt = now
    };

    public static User CreateAdmin(string email, string nickname, string passwordHash, DateTimeOffset now)
    {
        var admin = Register(email, nickname, passwordHash, now);
        admin.Role = UserRole.Admin;
        admin.EmailConfirmedAt = now;
        return admin;
    }

    public void ConfirmEmail(DateTimeOffset now) => EmailConfirmedAt ??= now;

    public void UpdateProfile(string nickname, string? phone, string? city, bool emailNotifications)
    {
        Nickname = nickname;
        Phone = phone;
        City = city;
        EmailNotifications = emailNotifications;
    }

    public void SetAvatar(string? avatarKey) => AvatarKey = avatarKey;

    public void ChangePasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void Block(long adminId, string reason, DateTimeOffset now)
    {
        if (IsBlocked)
        {
            return;
        }

        Status = UserStatus.Blocked;
        Raise(new UserBlocked(Id, adminId, reason, now));
    }

    public void Unblock() => Status = UserStatus.Active;
}
