using Bunq.Core.Models;
using Bunq.Infrastructure.Services.Persistence;
using NUnit.Framework;

namespace Bunq.Tests.Infrastructure;

public sealed class JsonBunqContextRepositoryServiceTests
{
    [Test]
    public void GetPath_UsesEnvironmentOverride_WhenProvided()
    {
        // Arrange
        var original = Environment.GetEnvironmentVariable("BUNQ_CONTEXT_FILE");
        var tempPath = Path.Combine(Path.GetTempPath(), $"bunq-context-{Guid.NewGuid():N}.json");
        Environment.SetEnvironmentVariable("BUNQ_CONTEXT_FILE", tempPath);
        var store = new JsonBunqContextRepositoryService();

        try
        {
            // Act
            var actual = store.GetPath();

            // Assert
            Assert.That(actual, Is.EqualTo(Path.GetFullPath(tempPath)));
        }
        finally
        {
            Environment.SetEnvironmentVariable("BUNQ_CONTEXT_FILE", original);
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Test]
    public async Task SaveLoadDelete_PerformsRoundTrip()
    {
        // Arrange
        var original = Environment.GetEnvironmentVariable("BUNQ_CONTEXT_FILE");
        var tempPath = Path.Combine(Path.GetTempPath(), $"bunq-context-{Guid.NewGuid():N}.json");
        Environment.SetEnvironmentVariable("BUNQ_CONTEXT_FILE", tempPath);
        var store = new JsonBunqContextRepositoryService();
        var expected = new BunqContext
        {
            InstallationToken = "installation-token",
            SessionToken = "session-token",
            UserId = 1234,
            DeviceRegistered = true,
            PublicKeyPem = "public",
            PrivateKeyPem = "private",
            ServerPublicKeyPem = "server-public"
        };

        try
        {
            // Act
            await store.SaveAsync(expected);
            var loaded = await store.LoadAsync();
            await store.DeleteAsync();

            // Assert
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.InstallationToken, Is.EqualTo(expected.InstallationToken));
            Assert.That(loaded.SessionToken, Is.EqualTo(expected.SessionToken));
            Assert.That(loaded.UserId, Is.EqualTo(expected.UserId));
            Assert.That(loaded.DeviceRegistered, Is.EqualTo(expected.DeviceRegistered));
            Assert.That(File.Exists(tempPath), Is.False);
        }
        finally
        {
            Environment.SetEnvironmentVariable("BUNQ_CONTEXT_FILE", original);
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
