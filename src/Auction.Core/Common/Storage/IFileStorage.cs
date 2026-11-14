// Object storage for lot photos, avatars and donation receipts.
namespace Auction.Core.Common.Storage;

public interface IFileStorage
{
    Task PutAsync(string key, Stream content, string contentType, CancellationToken ct);

    Task DeleteAsync(string key, CancellationToken ct);

    string GetPublicUrl(string key);
}
