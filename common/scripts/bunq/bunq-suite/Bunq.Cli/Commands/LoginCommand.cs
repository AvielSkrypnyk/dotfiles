using Bunq.Cli.Constants;
using Bunq.Cli.Helpers;
using Bunq.Cli.Models;
using Bunq.Cli.Services;
using Bunq.Core.Services;

namespace Bunq.Cli.Commands;

public sealed class LoginCommand
{
    private readonly LoginFlagsParserService _flagsParser;
    private readonly ApiKeyResolverService _apiKeyResolver;
    private readonly BunqAuthService _loginWorkflowService;

    public LoginCommand(
        LoginFlagsParserService flagsParser,
        ApiKeyResolverService apiKeyResolver,
        BunqAuthService loginWorkflowService)
    {
        _flagsParser = flagsParser;
        _apiKeyResolver = apiKeyResolver;
        _loginWorkflowService = loginWorkflowService;
    }

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var parseResult = _flagsParser.Parse(args);
        if (!parseResult.Success || parseResult.Data is null)
        {
            PrintError(parseResult.Error ?? "Could not parse login flags.");
            return;
        }

        var options = parseResult.Data;
        var apiKey = _apiKeyResolver.Resolve(options.ApiKey, interactivePrompt: true);
        if (apiKey is null)
        {
            PrintError($"API key is required. Provide {CliConstants.OptionApiKey} or {CliConstants.EnvApiKey}.");
            return;
        }

        var loginResult = await _loginWorkflowService.ExecuteAsync(
            options.BaseUrl,
            apiKey,
            options.DeviceDescription,
            options.GetPermittedIps(),
            options.ForceRecreateContext,
            cancellationToken);
        if (!loginResult.Success || loginResult.Data is null)
        {
            PrintError(BuildFriendlyError(loginResult.Error, options));
            return;
        }
        
        ColorConsole.WriteSuccess($"bunq login succeeded. User ID: {loginResult.Data.UserId}");
    }

    private static void PrintError(string message)
    {
        ColorConsole.WriteError($"Error: {message}");
    }

    private static string BuildFriendlyError(string? error, LoginOptions options)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return "Login failed.";
        }

        if (error.Contains("Incorrect API key or IP address", StringComparison.OrdinalIgnoreCase))
        {
            var permittedIp = options.GetPermittedIps().FirstOrDefault() ?? string.Empty;
            return CliMessages.BuildApiKeyIpHint(
                $"{error} Try leaving permitted IP empty (current IP), verify environment (sandbox/production), and confirm the key is active.",
                options.BaseUrl,
                permittedIp);
        }

        return error;
    }
}
