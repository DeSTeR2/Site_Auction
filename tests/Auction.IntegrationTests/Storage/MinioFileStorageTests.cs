// MinioFileStorage and its readiness check against a real MinIO container.
using System.Text;
using Auction.Core.Common.Storage;
using Auction.Infrastructure.Storage;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Auction.IntegrationTests.Storage;

public sealed class MinioFileStorageTests : IAsyncLifetime
{
    private const string AccessKey = "integration-user";
    private const string SecretKey = "integration-secret";
    private const string Bucket = "media";

    private readonly IContainer minio = new ContainerBuilder()
        .WithImage("cgr.dev/chainguard/minio:latest")
        .WithCommand("server", "/data")
        .WithEnvironment("MINIO_ROOT_USER", AccessKey)
        .WithEnvironment("MINIO_ROOT_PASSWORD", SecretKey)
        .WithPortBinding(9000, true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request.ForPort(9000).ForPath("/minio/health/live")))
        .Build();

    private IMinioClient client = null!;
    private IFileStorage storage = null!;

    public async Task InitializeAsync()
    {
        await minio.StartAsync();
        var endpoint = $"{minio.Hostname}:{minio.GetMappedPublicPort(9000)}";

        client = new MinioClient().WithEndpoint(endpoint).WithCredentials(AccessKey, SecretKey).Build();
        await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(Bucket));

        storage = FileStorageFactory.Create(new StorageOptions
        {
            Provider = FileStorageFactory.Minio,
            PublicBaseUrl = "/media",
            Minio = new MinioStorageOptions { Endpoint = endpoint, AccessKey = AccessKey, SecretKey = SecretKey, Bucket = Bucket }
        });
    }

    public async Task DisposeAsync() => await minio.DisposeAsync();

    [Fact]
    public async Task Uploaded_file_is_stored_with_its_content_type()
    {
        var content = Encoding.UTF8.GetBytes("photo bytes");

        await storage.PutAsync("lots/1/large.webp", new MemoryStream(content), "image/webp", CancellationToken.None);

        var stat = await client.StatObjectAsync(new StatObjectArgs().WithBucket(Bucket).WithObject("lots/1/large.webp"));
        Assert.Equal(content.Length, stat.Size);
        Assert.Equal("image/webp", stat.ContentType);
        Assert.Equal("/media/lots/1/large.webp", storage.GetPublicUrl("lots/1/large.webp"));
    }

    [Fact]
    public async Task Readiness_check_sees_the_bucket()
    {
        var healthy = await CheckHealthAsync(Bucket);
        var missingBucket = await CheckHealthAsync("not-created");

        Assert.Equal(HealthStatus.Healthy, healthy.Status);
        Assert.Equal(HealthStatus.Unhealthy, missingBucket.Status);
    }

    [Fact]
    public async Task Deleted_file_is_gone()
    {
        await storage.PutAsync("lots/2/thumb.webp", new MemoryStream([1, 2, 3]), "image/webp", CancellationToken.None);

        await storage.DeleteAsync("lots/2/thumb.webp", CancellationToken.None);

        await Assert.ThrowsAsync<ObjectNotFoundException>(() =>
            client.StatObjectAsync(new StatObjectArgs().WithBucket(Bucket).WithObject("lots/2/thumb.webp")));
    }

    private Task<HealthCheckResult> CheckHealthAsync(string bucket)
    {
        var options = new StorageOptions
        {
            Provider = FileStorageFactory.Minio,
            Minio = new MinioStorageOptions
            {
                Endpoint = $"{minio.Hostname}:{minio.GetMappedPublicPort(9000)}",
                AccessKey = AccessKey,
                SecretKey = SecretKey,
                Bucket = bucket
            }
        };

        return new MinioHealthCheck(Options.Create(options)).CheckHealthAsync(new HealthCheckContext());
    }
}
