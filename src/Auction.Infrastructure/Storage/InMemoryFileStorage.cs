// Process-local IFileStorage for tests and local runs without MinIO.
using System.Collections.Concurrent;
using Auction.Core.Common.Storage;

namespace Auction.Infrastructure.Storage;

public sealed class InMemoryFileStorage(StorageOptions options) : IFileStorage
{
    private readonly ConcurrentDictionary<string, StoredFile> files = new();

    public async Task PutAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        files[key] = new StoredFile(buffer.ToArray(), contentType);
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        files.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string key) => $"{options.PublicBaseUrl.TrimEnd('/')}/{key}";

    public bool TryGet(string key, out StoredFile file) => files.TryGetValue(key, out file!);
}

public sealed record StoredFile(byte[] Content, string ContentType);
