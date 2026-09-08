using System.Text.Json.Serialization;

namespace Bunq.Core.Models;

public sealed class InstallationResponse
{
    [JsonPropertyName("Response")]
    public List<InstallationItem>? Response { get; set; }
}

public sealed class InstallationItem
{
    [JsonPropertyName("Token")]
    public TokenContainer? Token { get; set; }

    [JsonPropertyName("ServerPublicKey")]
    public ServerPublicKeyContainer? ServerPublicKey { get; set; }
}

public sealed class TokenContainer
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}

public sealed class ServerPublicKeyContainer
{
    [JsonPropertyName("server_public_key")]
    public string? ServerPublicKey { get; set; }
}
