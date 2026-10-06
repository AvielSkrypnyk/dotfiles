namespace Bunq.Infrastructure.Services.Persistence;

public interface IContextFilePermissionStrategy
{
    bool Supports(PlatformKind platform);
    FileStream OpenSecureWriteStream(string path);
    void EnsureSecurePermissions(string path);
}
