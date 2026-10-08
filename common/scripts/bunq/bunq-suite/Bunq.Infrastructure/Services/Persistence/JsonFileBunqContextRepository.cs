using System.Text.Json;
using Bunq.Core.Interfaces;
using Bunq.Core.Models;

namespace Bunq.Infrastructure.Services.Persistence;

public class JsonFileBunqContextRepository : IBunqContextRepository
{
    private const string ContextFileEnvironmentVariable = "BUNQ_CONTEXT_FILE";
    private readonly IContextFileSecurityService _fileSecurityService;

    public JsonFileBunqContextRepository(IContextFileSecurityService fileSecurityService)
    {
        _fileSecurityService = fileSecurityService;
    }

    public string GetFilePath()
    {
        var configuredPath = Environment.GetEnvironmentVariable(ContextFileEnvironmentVariable);
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
        var path = GetFilePath();
        var json = JsonSerializer.Serialize(context, new JsonSerializerOptions { WriteIndented = true });

        _fileSecurityService.RestrictToCurrentUser(path);

        await using var stream = _fileSecurityService.OpenSecureWriteStream(path);
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        _fileSecurityService.RestrictToCurrentUser(path);
    }

    public async Task<BunqContext?> LoadAsync(CancellationToken ct = default)
    {
        var path = GetFilePath();

        if (!File.Exists(path))
            return null;

        _fileSecurityService.RestrictToCurrentUser(path);

        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<BunqContext>(json);
    }

    public Task DeleteAsync(CancellationToken ct = default)
    {
        var path = GetFilePath();
        
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}
