// MinIO readiness check without a running MinIO: skipped for other providers, unhealthy when unreachable.
using Auction.Infrastructure.Storage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Auction.UnitTests.Infrastructure;

public class MinioHealthCheckTests
{
    [Fact]
    public async Task Other_provider_is_reported_healthy()
    {
        var check = new MinioHealthCheck(Options.Create(new StorageOptions { Provider = FileStorageFactory.InMemory }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task Unreachable_minio_is_reported_unhealthy()
    {
        var options = new StorageOptions
        {
            Provider = FileStorageFactory.Minio,
            Minio = new MinioStorageOptions { Endpoint = "127.0.0.1:1", AccessKey = "user", SecretKey = "secret-key" }
        };
        var check = new MinioHealthCheck(Options.Create(options));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
