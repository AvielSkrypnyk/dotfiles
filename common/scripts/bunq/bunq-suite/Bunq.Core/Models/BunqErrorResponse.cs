using System.Text.Json.Serialization;

namespace Bunq.Core.Models;

public sealed class BunqErrorResponse
{
    [JsonPropertyName("Error")]
    public List<BunqErrorDetail>? Error { get; set; }
}

public sealed class BunqErrorDetail
{
    [JsonPropertyName("error_description")]
    public string? Description { get; set; }
}
