// Chooses the IFileStorage implementation from configuration; an unknown provider stops the application at startup.
using Auction.Core.Common.Storage;
using Minio;

namespace Auction.Infrastructure.Storage;

public static class FileStorageFactory
{
    public const string Minio = "Minio";
    public const string InMemory = "InMemory";

    public static IFileStorage Create(StorageOptions options) => options.Provider switch
    {
        Minio => new MinioFileStorage(BuildMinioClient(options.Minio), options),
        InMemory => new InMemoryFileStorage(options),
        _ => throw new InvalidOperationException($"Unknown storage provider '{options.Provider}'.")
    };

    internal static IMinioClient BuildMinioClient(MinioStorageOptions minio) => new MinioClient()
        .WithEndpoint(minio.Endpoint)
        .WithCredentials(minio.AccessKey, minio.SecretKey)
        .WithSSL(minio.UseSsl)
        .Build();
}
