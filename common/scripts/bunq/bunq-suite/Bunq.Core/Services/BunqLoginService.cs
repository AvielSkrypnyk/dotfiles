using Bunq.Core.Common;
using Bunq.Core.Interfaces;
using Bunq.Core.Models;

namespace Bunq.Core.Services;

public sealed class BunqLoginService
{
    private readonly IBunqContextRepository _contextRepository;
    private readonly IRsaService _rsaService;
    private readonly IBunqApiService _bunqApiService;

    public BunqLoginService(
        IBunqContextRepository contextRepository,
        IRsaService rsaService,
        IBunqApiService bunqApiService)
    {
        _contextRepository = contextRepository;
        _rsaService = rsaService;
        _bunqApiService = bunqApiService;
    }

    public async Task<OperationResult<BunqContext>> LoginAsync(
        string baseUrl,
        string apiKey,
        string deviceDescription,
        string[] permittedIps,
        bool forceRecreateContext,
        CancellationToken cancellationToken = default)
    {
        var context = await _contextRepository.LoadAsync(cancellationToken);
        if (forceRecreateContext || NeedsFreshInstallation(context))
        {
            var installation = await CreateInstallationAsync(baseUrl, forceRecreateContext, cancellationToken);
            if (!installation.Success || installation.Data is null)
            {
                return installation;
            }

            context = installation.Data;
        }

        if (context is null || string.IsNullOrWhiteSpace(context.InstallationToken))
        {
            return OperationResult<BunqContext>.Fail(
                MessageTypeEnum.Exception,
                "Missing_Installation",
                "Installation token is missing.");
        }

        if (!context.DeviceRegistered || forceRecreateContext)
        {
            var registration = await RegisterDeviceAsync(
                baseUrl,
                apiKey,
                deviceDescription,
                permittedIps,
                cancellationToken);
            if (!registration.Success || registration.Data is null)
            {
                return registration;
            }
        }

        var session = await CreateSessionAsync(baseUrl, apiKey, cancellationToken);
        if (session.Success && session.Data is not null)
        {
            return session;
        }

        if (forceRecreateContext)
        {
            return session;
        }

        var forcedInstallation = await CreateInstallationAsync(baseUrl, true, cancellationToken);
        if (!forcedInstallation.Success || forcedInstallation.Data is null)
        {
            return forcedInstallation;
        }

        var forcedRegistration = await RegisterDeviceAsync(
            baseUrl,
            apiKey,
            deviceDescription,
            permittedIps,
            cancellationToken);
        if (!forcedRegistration.Success || forcedRegistration.Data is null)
        {
            return forcedRegistration;
        }

        return await CreateSessionAsync(baseUrl, apiKey, cancellationToken);
    }

    public async Task<OperationResult<BunqContext>> CreateInstallationAsync(
        string baseUrl,
        bool forceNewKeys,
        CancellationToken cancellationToken = default)
    {
        var context = await _contextRepository.LoadAsync(cancellationToken);
        var shouldGenerateKeys = forceNewKeys ||
                                 context is null ||
                                 string.IsNullOrWhiteSpace(context.PublicKeyPem) ||
                                 string.IsNullOrWhiteSpace(context.PrivateKeyPem);

        if (shouldGenerateKeys)
        {
            var keyPair = _rsaService.GenerateKeyPair();
            context = new BunqContext
            {
                PublicKeyPem = keyPair.PublicKeyPem,
                PrivateKeyPem = keyPair.PrivateKeyPem
            };
        }

        if (context is null)
        {
            return OperationResult<BunqContext>.Fail(
                MessageTypeEnum.Exception,
                "Context_Missing",
                "Failed to initialize bunq context.");
        }

        var installationResult = await _bunqApiService.CreateInstallationAsync(
            baseUrl,
            context.PublicKeyPem,
            cancellationToken);
        if (!installationResult.Success || installationResult.Data is null)
        {
            return OperationResult<BunqContext>.Fail(
                installationResult.MessageType,
                installationResult.Code ?? "Installation_Failed",
                installationResult.Error ?? "Installation failed.");
        }

        var updated = new BunqContext
        {
            PublicKeyPem = context.PublicKeyPem,
            PrivateKeyPem = context.PrivateKeyPem,
            InstallationToken = installationResult.Data.Token,
            ServerPublicKeyPem = installationResult.Data.ServerPublicKeyPem,
            DeviceRegistered = false,
            SessionToken = string.Empty,
            UserId = 0
        };

        await _contextRepository.SaveAsync(updated);
        return OperationResult<BunqContext>.Ok(updated);
    }

    public async Task<OperationResult<BunqContext>> RegisterDeviceAsync(
        string baseUrl,
        string apiKey,
        string description,
        string[] permittedIps,
        CancellationToken cancellationToken = default)
    {
        var context = await _contextRepository.LoadAsync(cancellationToken);
        if (context is null || string.IsNullOrWhiteSpace(context.InstallationToken))
        {
            return OperationResult<BunqContext>.Fail(
                MessageTypeEnum.Exception,
                "Missing_Installation",
                "Installation is required before device registration.");
        }

        if (string.IsNullOrWhiteSpace(context.PrivateKeyPem) || string.IsNullOrWhiteSpace(context.ServerPublicKeyPem))
        {
            return OperationResult<BunqContext>.Fail(
                MessageTypeEnum.Exception,
                "Missing_Crypto_Material",
                "Installation keys are missing. Re-run installation to refresh local context.");
        }

        var registerResult = await _bunqApiService.RegisterDeviceAsync(
            baseUrl,
            context.InstallationToken,
            context.PrivateKeyPem,
            context.ServerPublicKeyPem,
            apiKey,
            description,
            permittedIps,
            cancellationToken);
        if (!registerResult.Success)
        {
            return OperationResult<BunqContext>.Fail(
                registerResult.MessageType,
                registerResult.Code ?? "Device_Registration_Failed",
                registerResult.Error ?? "Device registration failed.");
        }

        context.DeviceRegistered = true;
        await _contextRepository.SaveAsync(context);
        return OperationResult<BunqContext>.Ok(context);
    }

    public async Task<OperationResult<BunqContext>> CreateSessionAsync(
        string baseUrl,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var context = await _contextRepository.LoadAsync(cancellationToken);
        if (context is null || string.IsNullOrWhiteSpace(context.InstallationToken))
        {
            return OperationResult<BunqContext>.Fail(
                MessageTypeEnum.Exception,
                "Missing_Installation",
                "Installation is required before creating a session.");
        }

        if (string.IsNullOrWhiteSpace(context.PrivateKeyPem) || string.IsNullOrWhiteSpace(context.ServerPublicKeyPem))
        {
            return OperationResult<BunqContext>.Fail(
                MessageTypeEnum.Exception,
                "Missing_Crypto_Material",
                "Installation keys are missing. Re-run installation to refresh local context.");
        }

        var sessionResult = await _bunqApiService.CreateSessionAsync(
            baseUrl,
            context.InstallationToken,
            context.PrivateKeyPem,
            context.ServerPublicKeyPem,
            apiKey,
            cancellationToken);
        if (!sessionResult.Success || sessionResult.Data is null)
        {
            return OperationResult<BunqContext>.Fail(
                sessionResult.MessageType,
                sessionResult.Code ?? "Session_Failed",
                sessionResult.Error ?? "Session creation failed.");
        }

        context.SessionToken = sessionResult.Data.SessionToken;
        context.UserId = sessionResult.Data.UserId;
        await _contextRepository.SaveAsync(context);
        return OperationResult<BunqContext>.Ok(context);
    }

    private static bool NeedsFreshInstallation(BunqContext? existingContext)
    {
        return existingContext is null ||
               string.IsNullOrWhiteSpace(existingContext.InstallationToken) ||
               string.IsNullOrWhiteSpace(existingContext.PublicKeyPem) ||
               string.IsNullOrWhiteSpace(existingContext.PrivateKeyPem);
    }
}
