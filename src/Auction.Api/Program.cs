// Application entry point: composes the four modules, infrastructure and the HTTP pipeline.
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

app.Run();

public partial class Program;
