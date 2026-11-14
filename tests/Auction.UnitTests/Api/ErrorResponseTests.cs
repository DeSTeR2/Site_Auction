// Error format of design/08-api.md §2: code, title, data, errors, traceId for every failure path.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Auction.UnitTests.Api;

public class ErrorResponseTests(AuctionApiFactory factory) : IClassFixture<AuctionApiFactory>
{
    [Fact]
    public async Task Result_error_is_mapped_to_problem_details()
    {
        var response = await factory.CreateClient().GetAsync("/api/test/conflict");
        var problem = await response.ReadProblemAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("lot.has_bids", problem.GetProperty("code").GetString());
        Assert.Equal("На лот уже зроблено ставку, редагування неможливе", problem.GetProperty("title").GetString());
        Assert.Equal("https://auction.example/errors/lot.has_bids", problem.GetProperty("type").GetString());
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task Unknown_api_route_returns_not_found_code()
    {
        var response = await factory.CreateClient().GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("not_found", await response.ReadCodeAsync());
    }

    [Fact]
    public async Task Unhandled_exception_returns_server_error_without_details()
    {
        var response = await factory.CreateClient().GetAsync("/api/test/throw");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("server.error", body);
        Assert.DoesNotContain("secret internal detail", body);
    }

    [Fact]
    public async Task Validator_failures_are_returned_as_field_errors()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/test/echo", new { title = "abc", amount = 0 });
        var problem = await response.ReadProblemAsync();
        var errors = problem.GetProperty("errors");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation.failed", problem.GetProperty("code").GetString());
        Assert.Equal("Назва: від 5 до 100 символів", errors.GetProperty("title")[0].GetString());
        Assert.Equal("Сума має бути більшою за 0", errors.GetProperty("amount")[0].GetString());
    }

    [Fact]
    public async Task Malformed_json_is_reported_as_validation_failure()
    {
        var content = new StringContent("{ \"title\": \"Валідна назва\", \"amount\": \"lots\" }", System.Text.Encoding.UTF8, "application/json");

        var response = await factory.CreateClient().PostAsync("/api/test/echo", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation.failed", await response.ReadCodeAsync());
    }

    [Fact]
    public async Task Valid_request_passes_validation()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/test/echo", new { title = "Валідна назва", amount = 5 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, body.GetProperty("amount").GetInt32());
    }

    [Fact]
    public async Task Liveness_endpoint_is_healthy()
    {
        var response = await factory.CreateClient().GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
