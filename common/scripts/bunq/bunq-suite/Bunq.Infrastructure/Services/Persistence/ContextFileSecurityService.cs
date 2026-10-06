namespace Bunq.Infrastructure.Services.Persistence;

public sealed class ContextFileSecurityService : IContextFileSecurityService
{
    private readonly IContextFilePermissionStrategy _permissionStrategy;

    public ContextFileSecurityService(
        IPlatformService platformService,
        IEnumerable<IContextFilePermissionStrategy> strategies)
    {
        var platform = platformService.Current;
        _permissionStrategy = strategies.FirstOrDefault(strategy => strategy.Supports(platform))
            ?? throw new PlatformNotSupportedException($"Unsupported operating system for secure credential file handling: {platform}.");
    }

    public FileStream OpenSecureWriteStream(string path)
    {
        return _permissionStrategy.OpenSecureWriteStream(path);
    }

    public void EnsureSecurePermissions(string path)
    {
        _permissionStrategy.EnsureSecurePermissions(path);
    }
}
