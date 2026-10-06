using Bunq.Cli.Constants;
using Bunq.Cli.Helpers;

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
            apiKey = MaskedInputReader.ReadSecret();
        }

        return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
    }
}
