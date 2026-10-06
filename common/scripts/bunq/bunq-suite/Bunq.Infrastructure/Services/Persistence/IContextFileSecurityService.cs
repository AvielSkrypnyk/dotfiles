namespace Bunq.Infrastructure.Services.Persistence;

public interface IContextFileSecurityService
{
    FileStream OpenSecureWriteStream(string path);
    void EnsureSecurePermissions(string path);
}
