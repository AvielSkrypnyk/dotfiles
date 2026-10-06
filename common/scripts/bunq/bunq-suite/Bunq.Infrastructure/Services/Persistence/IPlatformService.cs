namespace Bunq.Infrastructure.Services.Persistence;

public enum PlatformKind
{
    Windows,
    Linux,
    MacOS,
    Unknown
}

public interface IPlatformService
{
    PlatformKind Current { get; }
}
