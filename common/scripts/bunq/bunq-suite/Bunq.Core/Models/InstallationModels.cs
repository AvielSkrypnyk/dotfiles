namespace Bunq.Core.Models;

public sealed record InstallationRequest(string ClientPublicKey);

public sealed record InstallationResult(string Token, string ServerPublicKeyPem);
