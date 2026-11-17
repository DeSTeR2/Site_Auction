// Access policies of design/07 §4 answer with auth.required, access.forbidden or access.email_not_confirmed.
using System.Net;
using Auction.Core.Common.Enums;

namespace Auction.UnitTests.Api;

public class AccessPolicyTests(AuctionApiFactory factory) : IClassFixture<AuctionApiFactory>
{
    public static TheoryData<string, string?, HttpStatusCode, string?> Matrix => new()
    {
        { "/api/test/authenticated", null, HttpStatusCode.Unauthorized, "auth.required" },
        { "/api/test/authenticated", "user", HttpStatusCode.OK, null },
        { "/api/test/authenticated", "admin", HttpStatusCode.OK, null },
        { "/api/test/user-only", null, HttpStatusCode.Unauthorized, "auth.required" },
        { "/api/test/user-only", "user", HttpStatusCode.NoContent, null },
        { "/api/test/user-only", "admin", HttpStatusCode.Forbidden, "access.forbidden" },
        { "/api/test/admin", null, HttpStatusCode.Unauthorized, "auth.required" },
        { "/api/test/admin", "user", HttpStatusCode.Forbidden, "access.forbidden" },
        { "/api/test/admin", "admin", HttpStatusCode.NoContent, null },
        { "/api/test/email-confirmed", "user", HttpStatusCode.NoContent, null },
        { "/api/test/email-confirmed", "unconfirmed", HttpStatusCode.Forbidden, "access.email_not_confirmed" },
    };

    [Theory]
    [MemberData(nameof(Matrix))]
    public async Task Policy_matrix(string path, string? who, HttpStatusCode expectedStatus, string? expectedCode)
    {
        var client = who switch
        {
            null => factory.CreateClient(),
            "admin" => factory.CreateClientWithSession(factory.Sessions.Add(1, factory.Clock.GetUtcNow(), UserRole.Admin)),
            "unconfirmed" => factory.CreateClientWithSession(factory.Sessions.Add(3, factory.Clock.GetUtcNow(), emailConfirmed: false)),
            _ => factory.CreateClientWithSession(factory.Sessions.Add(2, factory.Clock.GetUtcNow()))
        };

        var response = await client.GetAsync(path);

        Assert.Equal(expectedStatus, response.StatusCode);
        if (expectedCode is not null)
        {
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(expectedCode, await response.ReadCodeAsync());
        }
    }
}
