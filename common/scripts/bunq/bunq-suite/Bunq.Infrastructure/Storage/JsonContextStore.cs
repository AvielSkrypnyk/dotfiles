using System.Text.Json;
using Bunq.Core.Interfaces;
using Bunq.Core.Models;

namespace Bunq.Infrastructure.Storage;

public class JsonContextStore : IContextStore
{
    private static string GetContextPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var directory = Path.Combine(appData, "bunq");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "context.json");
    }

    public async Task SaveAsync(BunqContext context)
    {
        var json = JsonSerializer.Serialize(context, new JsonSerializerOptions{ WriteIndented = true });
        await File.WriteAllTextAsync(GetContextPath(), json);
    }

    public async Task<BunqContext?> LoadAsync(CancellationToken ct = default)
    {
        var path = GetContextPath();

        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<BunqContext>(json);
    }

    public Task DeleteAsync(CancellationToken ct = default)
    {
        var path = GetContextPath();
        
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}
