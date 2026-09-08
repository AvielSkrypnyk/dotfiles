using Bunq.Core.Common;
using Bunq.Core.Models;

namespace Bunq.Core.Interfaces;

public interface IBunqClient
{
    Task<OperationResult<InstallationResult>> CreateInstallationAsync(string clientPublicKeyPem, CancellationToken ct);
    Task RegisterDeviceAsync(string installationToken, string apiKey);
    Task CreateSessionAsync(string installationToken);
}
