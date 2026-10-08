using Bunq.Cli.Constants;

namespace Bunq.Cli.Models;

public sealed class LoginSettings
{
    public string? ApiKey { get; set; }
    public bool ForceRecreateContext { get; set; }
    public string BaseUrl { get; set; } = CliConstants.BaseUrlProduction;
    public string DeviceDescription { get; set; } = CliConstants.DefaultDeviceDescription;
    public List<string> PermittedIps { get; } = [];

    public string[] GetPermittedIps()
    {
        return PermittedIps.Count == 0 ? [] : PermittedIps.ToArray();
    }
}
