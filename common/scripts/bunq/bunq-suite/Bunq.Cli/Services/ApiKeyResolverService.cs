using Bunq.Cli.Constants;

namespace Bunq.Cli.Services;

public sealed class ApiKeyResolverService
{
    public string? Resolve(string? optionApiKey, bool interactivePrompt)
    {
        var apiKey = optionApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable(CliConstants.EnvApiKey);
        }

        if (string.IsNullOrWhiteSpace(apiKey) && interactivePrompt)
        {
            Console.Write("Enter bunq API key: ");
            apiKey = Console.ReadLine();
        }

        return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
    }
}
