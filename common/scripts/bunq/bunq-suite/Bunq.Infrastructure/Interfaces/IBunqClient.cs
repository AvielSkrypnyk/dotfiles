using Bunq.Core.Models;

namespace Bunq.Infrastructure.Interfaces;

public interface IBunqClient
{
    Task<InstallationResult> CreateInstallationAsync(string clientPublicKeyPem, CancellationToken ct = default);
    Task RegisterDeviceAsync(string installationToken, string apiKey, CancellationToken ct = default);
    Task<SessionInfo> CreateSessionAsync(string installationToken, string apiKey, CancellationToken ct = default);
}
