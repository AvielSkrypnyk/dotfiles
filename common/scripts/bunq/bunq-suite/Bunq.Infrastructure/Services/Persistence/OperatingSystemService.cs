namespace Bunq.Infrastructure.Services.Persistence;

public sealed class OperatingSystemService : IOperatingSystemService
{
    public OperatingSystemKind Current =>
        OperatingSystem.IsWindows() ? OperatingSystemKind.Windows :
        OperatingSystem.IsLinux() ? OperatingSystemKind.Linux :
        OperatingSystem.IsMacOS() ? OperatingSystemKind.MacOS :
        OperatingSystemKind.Unknown;
}
