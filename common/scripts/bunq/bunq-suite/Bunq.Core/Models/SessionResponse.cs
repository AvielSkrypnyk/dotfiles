using System.Text.Json.Serialization;

namespace Bunq.Core.Models;

public sealed class SessionResponse
{
    [JsonPropertyName("Response")]
    public List<SessionItem>? Response { get; set; }
}

public sealed class SessionItem
{
    [JsonPropertyName("Token")]
    public TokenContainer? Token { get; set; }

    [JsonPropertyName("UserPerson")]
    public UserContainer? UserPerson { get; set; }

    [JsonPropertyName("UserCompany")]
    public UserContainer? UserCompany { get; set; }

    [JsonPropertyName("UserApiKey")]
    public UserContainer? UserApiKey { get; set; }
}

public sealed class UserContainer
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
