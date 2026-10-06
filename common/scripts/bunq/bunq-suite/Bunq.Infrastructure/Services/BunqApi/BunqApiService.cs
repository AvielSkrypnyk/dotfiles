using Bunq.Core.Common;
using Bunq.Core.Interfaces;
using Bunq.Core.Models;

namespace Bunq.Infrastructure.Services.BunqApi;

public sealed class BunqApiService : IBunqApiService
{
    private const int RequestTimeoutSeconds = 30;
    private readonly IRsaService _rsaService;

    public BunqApiService(IRsaService rsaService)
    {
        _rsaService = rsaService;
    }

    public async Task<OperationResult<InstallationResult>> CreateInstallationAsync(
        string baseUrl,
        string clientPublicKeyPem,
        CancellationToken ct = default)
    {
        using var httpClient = CreateHttpClient(baseUrl);
        var client = new BunqApiClientService(httpClient, _rsaService);
        return await client.CreateInstallationAsync(clientPublicKeyPem, ct);
    }

    public async Task<OperationResult> RegisterDeviceAsync(
        string baseUrl,
        string installationToken,
        string privateKeyPem,
        string serverPublicKeyPem,
        string apiKey,
        string description,
        string[] permittedIps,
        CancellationToken ct = default)
    {
        using var httpClient = CreateHttpClient(baseUrl);
        var client = new BunqApiClientService(httpClient, _rsaService);
        return await client.RegisterDeviceAsync(
            installationToken,
            privateKeyPem,
            serverPublicKeyPem,
            apiKey,
            description,
            permittedIps,
            ct);
    }

    public async Task<OperationResult<SessionInfo>> CreateSessionAsync(
        string baseUrl,
        string installationToken,
        string privateKeyPem,
        string serverPublicKeyPem,
        string apiKey,
        CancellationToken ct = default)
    {
        using var httpClient = CreateHttpClient(baseUrl);
        var client = new BunqApiClientService(httpClient, _rsaService);
        return await client.CreateSessionAsync(installationToken, privateKeyPem, serverPublicKeyPem, apiKey, ct);
    }

    private static HttpClient CreateHttpClient(string baseUrl)
    {
        return new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds)
        };
    }
}
