// IFileStorage over a MinIO bucket that the reverse proxy serves publicly under /media.
using Auction.Core.Common.Storage;
using Minio;
using Minio.DataModel.Args;

namespace Auction.Infrastructure.Storage;

public sealed class MinioFileStorage(IMinioClient client, StorageOptions options) : IFileStorage
{
    public async Task PutAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        var args = new PutObjectArgs()
            .WithBucket(options.Minio.Bucket)
            .WithObject(key)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType);

        await client.PutObjectAsync(args, ct);
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(options.Minio.Bucket)
            .WithObject(key);

        await client.RemoveObjectAsync(args, ct);
    }

    public string GetPublicUrl(string key) => $"{options.PublicBaseUrl.TrimEnd('/')}/{key}";
}
