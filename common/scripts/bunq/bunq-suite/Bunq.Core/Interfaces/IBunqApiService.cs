using Bunq.Core.Common;
using Bunq.Core.Models;

namespace Bunq.Core.Interfaces;

public interface IBunqApiService
{
    Task<OperationResult<InstallationResult>> CreateInstallationAsync(
        string baseUrl,
        string clientPublicKeyPem,
        CancellationToken ct = default);

    Task<OperationResult> RegisterDeviceAsync(
        string baseUrl,
        string installationToken,
        string privateKeyPem,
        string serverPublicKeyPem,
        string apiKey,
        string description,
        string[] permittedIps,
        CancellationToken ct = default);

    Task<OperationResult<SessionInfo>> CreateSessionAsync(
        string baseUrl,
        string installationToken,
        string privateKeyPem,
        string serverPublicKeyPem,
        string apiKey,
        CancellationToken ct = default);
}
