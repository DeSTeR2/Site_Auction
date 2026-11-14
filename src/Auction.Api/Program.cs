// Application entry point: composes modules and the HTTP pipeline, or runs a deployment command (migrate, seed).
using Auction.Api.Cli;
using Auction.Api.Infrastructure;
using Auction.Api.Infrastructure.Authentication;
using Auction.Api.Infrastructure.Security;
using Auction.Core.Bidding;
using Auction.Core.Common;
using Auction.Core.Lots;
using Auction.Core.Platform;
using Auction.Core.Platform.Contracts;
using Auction.Core.Users;
using Auction.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuctionApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRequestSecurity();
builder.Services.AddSessionAuthentication(builder.Configuration);
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services
    .AddPlatform()
    .AddUsers()
    .AddLots()
    .AddBidding()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (args is [var command, ..] && CliCommands.IsKnown(command))
{
    return await CliCommands.RunAsync(app, command);
}

app.UseExceptionHandler();
app.UseMiddleware<OriginCheckMiddleware>();
app.UseRateLimiter();
app.UseMiddleware<VisitorCookieMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(InfrastructureModule.ReadinessTag)
});
app.MapFallback("/api/{**path}", context => context.WriteProblemAsync(AppErrors.NotFound));

await app.RunAsync();
return 0;

public partial class Program;
