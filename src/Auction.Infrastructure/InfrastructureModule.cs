// Registers persistence and file storage with their readiness checks, domain event dispatch and email.
using Auction.Core.Common.Email;
using Auction.Core.Common.Persistence;
using Auction.Core.Common.Storage;
using Auction.Infrastructure.Email;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.DomainEvents;
using Auction.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Auction.Infrastructure;

public static class InfrastructureModule
{
    public const string ReadinessTag = "ready";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        AddPersistence(services, configuration);
        AddStorage(services);
        AddEmail(services);
        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<PendingAfterCommitEvents>();
        services.AddScoped<DomainEventsInterceptor>();
        services.AddScoped<AfterCommitInterceptor>();

        services.AddDbContext<AuctionDbContext>((sp, options) => options
            .UseNpgsql(configuration.GetConnectionString("Postgres"))
            .AddInterceptors(
                sp.GetRequiredService<DomainEventsInterceptor>(),
                sp.GetRequiredService<AfterCommitInterceptor>()));
        services.AddScoped<IAuctionDbContext>(sp => sp.GetRequiredService<AuctionDbContext>());

        services.AddHealthChecks().AddDbContextCheck<AuctionDbContext>("postgres", tags: [ReadinessTag]);
    }

    private static void AddStorage(IServiceCollection services)
    {
        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IFileStorage>(sp =>
            FileStorageFactory.Create(sp.GetRequiredService<IOptions<StorageOptions>>().Value));

        services.AddSingleton<MinioHealthCheck>();
        services.AddHealthChecks().AddCheck<MinioHealthCheck>("minio", tags: [ReadinessTag]);
    }

    private static void AddEmail(IServiceCollection services)
    {
        services.AddOptions<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IEmailSender>(sp =>
            EmailSenderFactory.Create(sp.GetRequiredService<IOptions<EmailOptions>>().Value));
    }
}
