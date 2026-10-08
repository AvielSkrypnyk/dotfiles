using Bunq.Core.Models;

namespace Bunq.Core.Interfaces;

public interface IBunqContextRepository
{
    string GetFilePath();
    Task SaveAsync(BunqContext context);
    Task<BunqContext?> LoadAsync(CancellationToken ct = default);
    Task DeleteAsync(CancellationToken ct = default);
}
