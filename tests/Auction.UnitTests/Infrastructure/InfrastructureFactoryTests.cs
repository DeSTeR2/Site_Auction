// File storage and email sender factories choose implementations by configured provider.
using Auction.Core.Common.Email;
using Auction.Infrastructure.Email;
using Auction.Infrastructure.Storage;
using Microsoft.Extensions.Time.Testing;

namespace Auction.UnitTests.Infrastructure;

public class InfrastructureFactoryTests
{
    [Fact]
    public void Storage_provider_selects_implementation()
    {
        Assert.IsType<InMemoryFileStorage>(FileStorageFactory.Create(new StorageOptions { Provider = FileStorageFactory.InMemory }));
        var minio = new StorageOptions
        {
            Provider = FileStorageFactory.Minio,
            Minio = new MinioStorageOptions { AccessKey = "test-access", SecretKey = "test-secret" }
        };

        Assert.IsType<MinioFileStorage>(FileStorageFactory.Create(minio));
    }

    [Fact]
    public void Email_provider_selects_implementation()
    {
        Assert.IsType<SmtpEmailSender>(EmailSenderFactory.Create(new EmailOptions { Provider = EmailSenderFactory.Smtp }, TimeProvider.System));
        Assert.IsType<FileEmailSender>(EmailSenderFactory.Create(new EmailOptions { Provider = EmailSenderFactory.File }, TimeProvider.System));
    }

    [Fact]
    public async Task File_sender_names_file_by_injected_clock()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"mail-drop-{Guid.NewGuid():N}");
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 17, 9, 30, 15, 250, TimeSpan.Zero));
        var sender = new FileEmailSender(new EmailOptions { Provider = EmailSenderFactory.File, FileDirectory = directory }, clock);

        try
        {
            await sender.SendAsync(new EmailMessage("user@example.com", "Тема", "<p>Текст</p>"), CancellationToken.None);

            var file = Assert.Single(Directory.GetFiles(directory, "*.eml"));
            Assert.StartsWith("20261117-093015250-", Path.GetFileName(file));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Unknown_provider_fails_fast()
    {
        Assert.Throws<InvalidOperationException>(() => FileStorageFactory.Create(new StorageOptions { Provider = "Disk" }));
        Assert.Throws<InvalidOperationException>(() => EmailSenderFactory.Create(new EmailOptions { Provider = "Pigeon" }, TimeProvider.System));
    }

    [Fact]
    public async Task In_memory_storage_keeps_file_and_builds_public_url()
    {
        var storage = new InMemoryFileStorage(new StorageOptions { PublicBaseUrl = "/media/" });

        await storage.PutAsync("lots/a.webp", new MemoryStream([1, 2, 3]), "image/webp", CancellationToken.None);

        Assert.True(storage.TryGet("lots/a.webp", out var file));
        Assert.Equal("image/webp", file.ContentType);
        Assert.Equal("/media/lots/a.webp", storage.GetPublicUrl("lots/a.webp"));
    }
}
