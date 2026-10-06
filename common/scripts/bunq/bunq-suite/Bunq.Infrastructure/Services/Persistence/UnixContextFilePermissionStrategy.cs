using System.Diagnostics.CodeAnalysis;

namespace Bunq.Infrastructure.Services.Persistence;

public sealed class UnixContextFilePermissionStrategy : IContextFilePermissionStrategy
{
    private const UnixFileMode OwnerReadWrite = UnixFileMode.UserRead | UnixFileMode.UserWrite;

    public bool Supports(PlatformKind platform) => platform is PlatformKind.Linux or PlatformKind.MacOS;

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Guarded by runtime OS checks.")]
    public FileStream OpenSecureWriteStream(string path)
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("Unix permission strategy can only run on Linux or macOS.");
        }

        return new FileStream(path, new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.None,
            UnixCreateMode = OwnerReadWrite
        });
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Guarded by runtime OS checks.")]
    public void EnsureSecurePermissions(string path)
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("Unix permission strategy can only run on Linux or macOS.");
        }

        if (!File.Exists(path))
        {
            return;
        }

        File.SetUnixFileMode(path, OwnerReadWrite);
    }
}
