// Every stored enum column has a CHECK constraint listing exactly the text values the application writes.
using Auction.Core.Common.Enums;
using Auction.Infrastructure.Persistence;
using Auction.IntegrationTests.Infrastructure;

namespace Auction.IntegrationTests.Persistence;

[Collection(PostgresCollection.Name)]
public class EnumConstraintTests(PostgresApiFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseTestHelper database = new(fixture);

    public static TheoryData<Type, string, string> Columns => new()
    {
        { typeof(UserRole), "users", "role" },
        { typeof(UserStatus), "users", "status" },
        { typeof(TokenPurpose), "user_tokens", "purpose" },
        { typeof(ComplaintReason), "complaints", "reason" },
        { typeof(ComplaintStatus), "complaints", "status" },
        { typeof(AdminActionType), "admin_actions", "action" },
        { typeof(LotStatus), "lots", "status" },
        { typeof(LotCondition), "lots", "condition" },
        { typeof(DonationLinkStatus), "donation_links", "status" },
        { typeof(NotificationType), "notifications", "type" },
        { typeof(EmailStatus), "notifications", "email_status" },
        { typeof(SuspiciousActivityType), "suspicious_activities", "type" }
    };

    public Task InitializeAsync() => database.MigrateAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [MemberData(nameof(Columns))]
    public async Task Check_constraint_lists_every_enum_value(Type enumType, string table, string column)
    {
        var definitions = await database.QueryColumnAsync(
            $"SELECT pg_get_constraintdef(oid) FROM pg_constraint WHERE contype = 'c' AND conname = 'ck_{table}_{column}'");
        var converter = SnakeCaseEnumConverter.For(enumType);

        var definition = Assert.Single(definitions);
        foreach (var value in Enum.GetValues(enumType))
        {
            if (value is UserRole.Guest)
            {
                continue;
            }

            Assert.Contains($"'{converter.ConvertToProvider(value)}'", definition);
        }
    }
}
