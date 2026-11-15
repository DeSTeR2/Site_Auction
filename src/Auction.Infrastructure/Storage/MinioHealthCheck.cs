// Readiness check: MinIO answers its liveness endpoint and the media bucket exists; healthy for other providers.
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Auction.Infrastructure.Storage;

public sealed class MinioHealthCheck(IOptions<StorageOptions> options) : IHealthCheck, IDisposable
{
    private readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(3) };
    private readonly Lazy<IMinioClient> client = new(() => FileStorageFactory.BuildMinioClient(options.Value.Minio));

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var storage = options.Value;
        if (storage.Provider != FileStorageFactory.Minio)
        {
            return HealthCheckResult.Healthy($"Storage provider is {storage.Provider}");
        }

        try
        {
            var scheme = storage.Minio.UseSsl ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            using var live = await http.GetAsync($"{scheme}://{storage.Minio.Endpoint}/minio/health/live", cancellationToken);
            if (!live.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy($"MinIO liveness returned {(int)live.StatusCode}");
            }

            var exists = await client.Value.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(storage.Minio.Bucket),
                cancellationToken);

            return exists
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Bucket '{storage.Minio.Bucket}' does not exist");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("MinIO is unreachable", ex);
        }
    }

    public void Dispose() => http.Dispose();
}
