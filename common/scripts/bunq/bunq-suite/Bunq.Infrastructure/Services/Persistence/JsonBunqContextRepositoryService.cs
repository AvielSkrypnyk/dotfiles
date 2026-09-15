using System.Text.Json;
using Bunq.Core.Interfaces;
using Bunq.Core.Models;

namespace Bunq.Infrastructure.Services.Persistence;

public class JsonBunqContextRepositoryService : IBunqContextRepository
{
    public string GetPath()
    {
        var configuredPath = Environment.GetEnvironmentVariable("BUNQ_CONTEXT_FILE");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            var absolutePath = Path.GetFullPath(configuredPath);
            var customDirectory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrWhiteSpace(customDirectory))
            {
                Directory.CreateDirectory(customDirectory);
            }

            return absolutePath;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var directory = Path.Combine(appData, "bunq");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "context.json");
    }

    public async Task SaveAsync(BunqContext context)
    {
        var json = JsonSerializer.Serialize(context, new JsonSerializerOptions{ WriteIndented = true });
        await File.WriteAllTextAsync(GetPath(), json);
    }

    public async Task<BunqContext?> LoadAsync(CancellationToken ct = default)
    {
        var path = GetPath();

        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<BunqContext>(json);
    }

    public Task DeleteAsync(CancellationToken ct = default)
    {
        var path = GetPath();
        
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}
