using Bunq.Cli.Constants;
using Bunq.Cli.Helpers;
using Bunq.Cli.Models;
using Bunq.Cli.Services;
using Bunq.Core.Services;

namespace Bunq.Cli.Commands;

public sealed class LoginCommand
{
    private readonly LoginOptionsParserService _optionsParser;
    private readonly ApiKeyResolverService _apiKeyResolver;
    private readonly BunqAuthService _loginWorkflowService;

    public LoginCommand(
        LoginOptionsParserService optionsParser,
        ApiKeyResolverService apiKeyResolver,
        BunqAuthService loginWorkflowService)
    {
        _optionsParser = optionsParser;
        _apiKeyResolver = apiKeyResolver;
        _loginWorkflowService = loginWorkflowService;
    }

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (!_optionsParser.TryParse(args, out var options, out var parseError))
        {
            PrintError(parseError ?? "Could not parse options.");
            return;
        }

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
