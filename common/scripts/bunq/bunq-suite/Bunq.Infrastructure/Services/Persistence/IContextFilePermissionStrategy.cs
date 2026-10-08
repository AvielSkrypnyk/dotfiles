namespace Bunq.Infrastructure.Services.Persistence;

public interface IContextFilePermissionStrategy
{
    bool Supports(OperatingSystemKind platform);
    FileStream OpenSecureWriteStream(string path);
    void RestrictToCurrentUser(string path);
}
