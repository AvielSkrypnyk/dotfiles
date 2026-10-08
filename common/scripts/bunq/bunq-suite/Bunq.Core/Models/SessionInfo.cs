namespace Bunq.Core.Models;

public sealed class SessionInfo
{
    public string SessionToken { get; set; } = string.Empty;
    public int UserId { get; set; }
}
