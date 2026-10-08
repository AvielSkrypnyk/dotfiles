using Bunq.Infrastructure.Services.Security;
using NUnit.Framework;

namespace Bunq.Tests.Infrastructure;

public sealed class RsaServiceTests
{
    [Test]
    public void GenerateKeyPair_ReturnsPublicAndPrivatePem()
    {
        // Arrange
        var service = new RsaService();

        // Act
        var keyPair = service.GenerateKeyPair();

        // Assert
        Assert.That(keyPair.PublicKeyPem, Does.Contain("BEGIN PUBLIC KEY"));
        Assert.That(keyPair.PrivateKeyPem, Does.Contain("BEGIN PRIVATE KEY"));
    }

    [Test]
    public void SignData_CreatesSignatureThatMatchesPublicKey()
    {
        // Arrange
        var service = new RsaService();
        var keyPair = service.GenerateKeyPair();
        const string payload = "bunq-signature-test";

        // Act
        var signature = service.SignData(payload, keyPair.PrivateKeyPem);

        // Assert
        var isValid = service.VerifyData(payload, signature, keyPair.PublicKeyPem);
        Assert.That(isValid, Is.True);
    }
}
