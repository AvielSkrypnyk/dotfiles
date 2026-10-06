namespace Bunq.Infrastructure.Services.Persistence;

public sealed class RuntimePlatformService : IPlatformService
{
    public PlatformKind Current =>
        OperatingSystem.IsWindows() ? PlatformKind.Windows :
        OperatingSystem.IsLinux() ? PlatformKind.Linux :
        OperatingSystem.IsMacOS() ? PlatformKind.MacOS :
        PlatformKind.Unknown;
}
