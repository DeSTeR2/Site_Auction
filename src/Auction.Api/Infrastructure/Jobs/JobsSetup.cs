// Hangfire on PostgreSQL with critical, default and maintenance queues and the admin-only dashboard at /admin/jobs.
using Auction.Api.Infrastructure.Authorization;
using Hangfire;
using Hangfire.PostgreSql;

namespace Auction.Api.Infrastructure.Jobs;

public static class JobsSetup
{
    public const string DashboardPath = "/admin/jobs";

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JobsOptions>()
            .BindConfiguration(JobsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = configuration.GetSection(JobsOptions.SectionName).Get<JobsOptions>() ?? new JobsOptions();
        var connectionString = configuration.GetConnectionString("Postgres");

        services.AddHangfire(hangfire =>
        {
            hangfire
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            switch (options.Storage)
            {
                case JobsOptions.PostgreSql:
                    hangfire.UsePostgreSqlStorage(
                        bootstrap => bootstrap.UseNpgsqlConnection(connectionString),
                        new PostgreSqlStorageOptions { SchemaName = "hangfire", PrepareSchemaIfNecessary = true });
                    break;
                case JobsOptions.InMemory:
                    hangfire.UseInMemoryStorage();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown job storage '{options.Storage}'.");
            }
        });

        services.AddHangfireServer(server =>
        {
            server.Queues = ["critical", "default", "maintenance"];
            server.WorkerCount = 5;
        });

        return services;
    }

    public static IEndpointRouteBuilder MapJobsDashboard(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapHangfireDashboard(DashboardPath, new DashboardOptions
            {
                Authorization = [],
                DashboardTitle = "Фонові задачі",
                AppPath = "/"
            })
            .RequireAuthorization(AuthPolicies.Admin);
        return endpoints;
    }
}
