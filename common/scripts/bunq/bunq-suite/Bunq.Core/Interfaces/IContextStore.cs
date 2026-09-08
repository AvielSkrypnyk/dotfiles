using Bunq.Core.Models;

namespace Bunq.Core.Interfaces;

public interface IContextStore
{
    Task SaveAsync(BunqContext context);
    Task<BunqContext?> LoadAsync(CancellationToken ct = default);
    Task DeleteAsync(CancellationToken ct = default);
}
