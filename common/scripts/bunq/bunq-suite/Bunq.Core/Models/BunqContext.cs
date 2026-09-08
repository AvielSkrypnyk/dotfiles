namespace Bunq.Core.Models;

public class BunqContext
{
    public string InstallationToken { get; init; } = "";
    public string SessionToken { get; set; } = "";
    public int UserId { get; set; }
    public string PrivateKeyPem { get; set; } = "";
    public string PublicKeyPem { get; set; } = "";
}
