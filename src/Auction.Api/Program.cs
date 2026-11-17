// Application entry point: composes the four modules, infrastructure and the HTTP pipeline.
using Auction.Api.Infrastructure;
using Auction.Core.Bidding;
using Auction.Core.Lots;
using Auction.Core.Platform;
using Auction.Core.Platform.Contracts;
using Auction.Core.Users;
using Auction.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ServerErrorExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services
    .AddPlatform()
    .AddUsers()
    .AddLots()
    .AddBidding()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<VisitorCookieMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
