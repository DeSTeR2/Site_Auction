// File storage settings bound to the "Storage" configuration section.
using System.ComponentModel.DataAnnotations;

namespace Auction.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    [Required]
    public string Provider { get; init; } = FileStorageFactory.Minio;

    [Required]
    public string PublicBaseUrl { get; init; } = "/media";

    public MinioStorageOptions Minio { get; init; } = new();
}

public sealed class MinioStorageOptions
{
    public string Endpoint { get; init; } = "minio:9000";

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public string Bucket { get; init; } = "media";

    public bool UseSsl { get; init; }
}
