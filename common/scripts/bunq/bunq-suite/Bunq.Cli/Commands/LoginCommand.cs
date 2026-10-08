using Bunq.Cli.Constants;
using Bunq.Cli.Helpers;
using Bunq.Cli.Models;
using Bunq.Cli.Services;
using Bunq.Core.Services;

namespace Bunq.Cli.Commands;

public sealed class LoginCommand
{
    private readonly LoginFlagsParserService _flagsParser;
    private readonly ApiKeyProviderService _apiKeyProvider;
    private readonly BunqLoginService _loginService;

    public LoginCommand(
        LoginFlagsParserService flagsParser,
        ApiKeyProviderService apiKeyProvider,
        BunqLoginService loginService)
    {
        _flagsParser = flagsParser;
        _apiKeyProvider = apiKeyProvider;
        _loginService = loginService;
    }

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var parseResult = _flagsParser.Parse(args);
        if (!parseResult.Success || parseResult.Data is null)
        {
            PrintError(parseResult.Error ?? "Could not parse login flags.");
            return;
        }

        var loginSettings = parseResult.Data;
        var apiKey = _apiKeyProvider.GetApiKey(loginSettings.ApiKey);
        if (apiKey is null)
        {
            PrintError($"API key is required. Provide {CliConstants.FlagApiKey} or {CliConstants.EnvVarApiKey}.");
            return;
        }

        var loginResult = await _loginService.LoginAsync(
            loginSettings.BaseUrl,
            apiKey,
            loginSettings.DeviceDescription,
            loginSettings.GetPermittedIps(),
            loginSettings.ForceRecreateContext,
            cancellationToken);
        if (!loginResult.Success || loginResult.Data is null)
        {
            PrintError(BuildFriendlyError(loginResult.Error, loginSettings));
            return;
        }
        
        ColorConsole.WriteSuccess($"bunq login succeeded. User ID: {loginResult.Data.UserId}");
    }

    private static void PrintError(string message)
    {
        ColorConsole.WriteError($"Error: {message}");
    }

    private static string BuildFriendlyError(string? error, LoginSettings loginSettings)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return "Login failed.";
        }

        if (error.Contains("Incorrect API key or IP address", StringComparison.OrdinalIgnoreCase))
        {
            var permittedIp = loginSettings.GetPermittedIps().FirstOrDefault() ?? string.Empty;
            return CliMessages.BuildApiKeyIpHint(
                $"{error} Try leaving permitted IP empty (current IP), verify environment (sandbox/production), and confirm the key is active.",
                loginSettings.BaseUrl,
                permittedIp);
        }

        return error;
    }
}
