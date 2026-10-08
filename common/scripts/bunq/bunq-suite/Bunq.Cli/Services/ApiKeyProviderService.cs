using Bunq.Cli.Constants;
using Bunq.Cli.Helpers;

namespace Bunq.Cli.Services;

/// <summary>
/// Gets the bunq API key from, in order: the --api-key flag, the BUNQ_API_KEY environment variable,
/// or by asking the user to type it (only when someone can type, not in scripts or piped input).
/// </summary>
public sealed class ApiKeyProviderService
{
    public string? GetApiKey(string? apiKeyFromFlag)
    {
        var apiKey = apiKeyFromFlag;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable(CliConstants.EnvVarApiKey);
        }

        if (string.IsNullOrWhiteSpace(apiKey) && CanAskUser())
        {
            Console.Write("Enter bunq API key: ");
            apiKey = MaskedInputReader.ReadSecret();
        }

        return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
    }

    // Console.ReadKey crashes when input is piped or redirected (scripts, CI), so only ask a real keyboard user.
    private static bool CanAskUser()
    {
        return !Console.IsInputRedirected;
    }
}
