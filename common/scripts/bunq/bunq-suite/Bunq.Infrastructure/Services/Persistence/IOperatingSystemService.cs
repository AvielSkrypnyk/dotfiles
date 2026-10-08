namespace Bunq.Infrastructure.Services.Persistence;

public enum OperatingSystemKind
{
    Windows,
    Linux,
    MacOS,
    Unknown
}

public interface IOperatingSystemService
{
    OperatingSystemKind Current { get; }
}
