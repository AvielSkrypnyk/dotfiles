using System.Security.AccessControl;
using System.Security.Principal;

namespace Bunq.Infrastructure.Services.Persistence;

public sealed class WindowsContextFilePermissionStrategy : IContextFilePermissionStrategy
{
    public bool Supports(OperatingSystemKind platform) => platform == OperatingSystemKind.Windows;

    public FileStream OpenSecureWriteStream(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows ACL strategy can only run on Windows.");
        }

        var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        RestrictToCurrentUser(path);
        return stream;
    }

    public void RestrictToCurrentUser(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows ACL strategy can only run on Windows.");
        }

        if (!File.Exists(path))
        {
            return;
        }

        var currentUser = WindowsIdentity.GetCurrent().User;
        if (currentUser is null)
        {
            throw new InvalidOperationException("Unable to resolve current Windows user for credential file ACL.");
        }

        var security = new FileSecurity();
        security.SetOwner(currentUser);
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(new FileSystemAccessRule(currentUser, FileSystemRights.FullControl, AccessControlType.Allow));
        var fileInfo = new FileInfo(path);
        fileInfo.SetAccessControl(security);
    }
}
