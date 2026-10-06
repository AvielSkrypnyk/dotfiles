using Bunq.Cli.Constants;
using Bunq.Cli.Helpers;
using Bunq.Cli.Models;
using Bunq.Core.Interfaces;
using Bunq.Core.Services;

namespace Bunq.Cli.Commands;

public sealed class MenuCommand
{
    private readonly ResetCommand _resetCommand;
    private readonly ContextPathCommand _contextPathCommand;
    private readonly IBunqContextRepository _contextStore;
    private readonly BunqAuthService _loginWorkflowService;

    private string? _apiKey;
    private string _baseUrl = CliConstants.BaseUrlSandbox;
    private string _deviceDescription = CliConstants.DefaultDeviceDescription;
    private string _permittedIp = string.Empty;

    public MenuCommand(
        ResetCommand resetCommand,
        ContextPathCommand contextPathCommand,
        IBunqContextRepository contextStore,
        BunqAuthService loginWorkflowService)
    {
        _resetCommand = resetCommand;
        _contextPathCommand = contextPathCommand;
        _contextStore = contextStore;
        _loginWorkflowService = loginWorkflowService;
        _apiKey = Environment.GetEnvironmentVariable(CliConstants.EnvApiKey);
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            PrintMenu();
            var choice = (Console.ReadLine() ?? "").Trim();

            try
            {
                var shouldExit = await ExecuteChoiceAsync(choice, cancellationToken);
                if (shouldExit)
                {
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                ColorConsole.WriteWarning(CliMessages.Canceled);
            }
            catch (Exception ex)
            {
                ColorConsole.WriteError($"Error: {ex.Message}");
            }
        }
    }

    private void PrintMenu()
    {
        ColorConsole.WriteLine(string.Empty);
        ColorConsole.WriteInfo(CliMessages.MenuTitle);
        ColorConsole.WriteLine("1. Easy setup (recommended)");
        ColorConsole.WriteLine("2. Paste API key (needed for handshake)");
        ColorConsole.WriteLine("3. Configure handshake settings");
        ColorConsole.WriteLine("4. Run installation step");
        ColorConsole.WriteLine("5. Run device registration step");
        ColorConsole.WriteLine("6. Run session creation step");
        ColorConsole.WriteLine("7. Show context file path");
        ColorConsole.WriteLine("8. Show login status");
        ColorConsole.WriteLine("9. Reset context");
        ColorConsole.WriteLine("0. Exit");
        ColorConsole.WriteLine(string.Empty);
        ColorConsole.Write(CliMessages.SelectionPrompt);
    }

    private void PasteApiKey()
    {
        ColorConsole.Write(CliMessages.ApiKeyPrompt);
        
        var entered = MaskedInputReader.ReadSecret();
        if (string.IsNullOrWhiteSpace(entered))
        {
            ColorConsole.WriteWarning(CliMessages.ApiKeyNotChanged);
            return;
        }

        var candidate = entered.Trim();
        if (!LooksLikeApiKey(candidate))
        {
            ColorConsole.WriteError(CliMessages.BuildApiKeyInvalidMessage(CliConstants.MinApiKeyLength));
            return;
        }

        _apiKey = candidate;
        ColorConsole.WriteSuccess(CliMessages.ApiKeySaved);
        ColorConsole.WriteInfo(CliMessages.ApiKeyFormatChecked);
    }

    private async Task EasySetupAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            PasteApiKey();
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            ColorConsole.WriteError($"Error: {CliMessages.ApiKeyRequired}");
            return;
        }

        PromptHandshakeConfiguration();
        await RunFullHandshakeAsync(cancellationToken);
    }

    private async Task ShowStatusAsync(CancellationToken cancellationToken)
    {
        var context = await _contextStore.LoadAsync(cancellationToken);
        if (context is null)
        {
            ColorConsole.WriteWarning(CliMessages.ContextMissing);
            return;
        }

        ColorConsole.WriteLine(string.Empty);
        var hasInstallation = !string.IsNullOrWhiteSpace(context.InstallationToken);
        var hasSession = !string.IsNullOrWhiteSpace(context.SessionToken);
        var hasUser = context.UserId > 0;
        var handshakeReady = hasInstallation && context.DeviceRegistered && hasSession && hasUser;

        ColorConsole.WriteInfo($"User ID: {(hasUser ? context.UserId : 0)}");
        ColorConsole.WriteEmbeddedColorLine($"Installation token: {ColorBool(hasInstallation)}");
        ColorConsole.WriteEmbeddedColorLine($"Device registered: {ColorBool(context.DeviceRegistered)}");
        ColorConsole.WriteEmbeddedColorLine($"Session token: {ColorBool(hasSession)}");
        ColorConsole.WriteEmbeddedColorLine($"Handshake ready: {ColorBool(handshakeReady)}");
        ColorConsole.WriteLine(string.Empty);
    }

    private async Task<bool> ExecuteChoiceAsync(string choice, CancellationToken cancellationToken)
    {
        switch (choice)
        {
            case "1":
                await EasySetupAsync(cancellationToken);
                return false;
            case "2":
                PasteApiKey();
                return false;
            case "3":
                PromptHandshakeConfiguration();
                return false;
            case "4":
                await RunInstallationStepAsync(cancellationToken);
                return false;
            case "5":
                await RunDeviceRegistrationStepAsync(cancellationToken);
                return false;
            case "6":
                await RunSessionStepAsync(cancellationToken);
                return false;
            case "7":
                await _contextPathCommand.ExecuteAsync();
                return false;
            case "8":
                await ShowStatusAsync(cancellationToken);
                return false;
            case "9":
                await _resetCommand.ExecuteAsync(cancellationToken);
                return false;
            case "0":
                return true;
            default:
                ColorConsole.WriteWarning(CliMessages.InvalidMenuChoice);
                return false;
        }
    }

    private void PromptHandshakeConfiguration()
    {
        ColorConsole.Write(CliMessages.EnvironmentPrompt);
        var environment = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        _baseUrl = environment == "production" ? CliConstants.BaseUrlProduction : CliConstants.BaseUrlSandbox;

        ColorConsole.Write(CliMessages.DeviceDescriptionPrompt(CliConstants.DefaultDeviceDescription));
        var description = (Console.ReadLine() ?? "").Trim();
        _deviceDescription = string.IsNullOrWhiteSpace(description) ? CliConstants.DefaultDeviceDescription : description;

        ColorConsole.Write(CliMessages.PermittedIpPrompt);
        _permittedIp = (Console.ReadLine() ?? "").Trim();
    }

    private async Task RunFullHandshakeAsync(CancellationToken cancellationToken)
    {
        var installed = await RunInstallationStepAsync(cancellationToken);
        if (!installed)
        {
            return;
        }

        var registered = await RunDeviceRegistrationStepAsync(cancellationToken);
        if (!registered)
        {
            return;
        }

        await RunSessionStepAsync(cancellationToken);
    }

    private async Task<bool> RunInstallationStepAsync(CancellationToken cancellationToken)
    {
        ColorConsole.WriteLine(string.Empty);
        var result = await _loginWorkflowService.RunInstallationAsync(_baseUrl, forceNewKeys: false, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            PrintStepResult(HandshakeStep.Installation, false);
            ColorConsole.WriteError($"Error: {BuildStepError(result.Error)}");
            ColorConsole.WriteLine(string.Empty);
            return false;
        }

        PrintStepResult(HandshakeStep.Installation, true);
        ColorConsole.WriteInfo($"Installation token: {result.Data.InstallationToken}");
        ColorConsole.WriteEmbeddedColorLine($"Server public key received: {ColorBool(!string.IsNullOrWhiteSpace(result.Data.ServerPublicKeyPem))}");
        ColorConsole.WriteLine(string.Empty);
        return true;
    }

    private async Task<bool> RunDeviceRegistrationStepAsync(CancellationToken cancellationToken)
    {
        ColorConsole.WriteLine(string.Empty);
        if (!TryEnsureApiKey())
        {
            PrintStepResult(HandshakeStep.DeviceRegistration, false);
            ColorConsole.WriteLine(string.Empty);
            return false;
        }

        var permittedIps = string.IsNullOrWhiteSpace(_permittedIp) ? Array.Empty<string>() : [_permittedIp];
        var result = await _loginWorkflowService.RunDeviceRegistrationAsync(
            _baseUrl,
            _apiKey!,
            _deviceDescription,
            permittedIps,
            cancellationToken);
        if (!result.Success || result.Data is null)
        {
            PrintStepResult(HandshakeStep.DeviceRegistration, false);
            ColorConsole.WriteError($"Error: {BuildStepError(result.Error)}");
            ColorConsole.WriteLine(string.Empty);
            return false;
        }

        PrintStepResult(HandshakeStep.DeviceRegistration, result.Data.DeviceRegistered);
        ColorConsole.WriteLine(string.Empty);
        return true;
    }

    private async Task<bool> RunSessionStepAsync(CancellationToken cancellationToken)
    {
        ColorConsole.WriteLine(string.Empty);
        if (!TryEnsureApiKey())
        {
            PrintStepResult(HandshakeStep.SessionCreation, false);
            ColorConsole.WriteLine(string.Empty);
            return false;
        }

        var result = await _loginWorkflowService.RunSessionCreationAsync(_baseUrl, _apiKey!, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            PrintStepResult(HandshakeStep.SessionCreation, false);
            ColorConsole.WriteError($"Error: {BuildStepError(result.Error)}");
            ColorConsole.WriteLine(string.Empty);
            return false;
        }

        PrintStepResult(HandshakeStep.SessionCreation, !string.IsNullOrWhiteSpace(result.Data.SessionToken));
        ColorConsole.WriteInfo($"Session token: {result.Data.SessionToken}");
        ColorConsole.WriteInfo($"User ID: {result.Data.UserId}");
        ColorConsole.WriteLine(string.Empty);
        return true;
    }

    private bool TryEnsureApiKey()
    {
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            return true;
        }

        PasteApiKey();
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            ColorConsole.WriteError($"Error: {CliMessages.ApiKeyRequired}");
            return false;
        }

        return true;
    }

    private static string ColorBool(bool value)
    {
        return value ? "[green]true[/green]" : "[red]false[/red]";
    }

    private static bool LooksLikeApiKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Length < CliConstants.MinApiKeyLength)
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];
            var isAllowed =
                (ch >= 'a' && ch <= 'z') ||
                (ch >= 'A' && ch <= 'Z') ||
                (ch >= '0' && ch <= '9') ||
                ch == '-' ||
                ch == '_';

            if (!isAllowed)
            {
                return false;
            }
        }

        return true;
    }

    private static string GetStepLabel(HandshakeStep step)
    {
        return step switch
        {
            HandshakeStep.Installation => "Installation step",
            HandshakeStep.DeviceRegistration => "Device registration step",
            HandshakeStep.SessionCreation => "Session creation step",
            _ => "Step"
        };
    }

    private void PrintStepResult(HandshakeStep step, bool success)
    {
        ColorConsole.WriteEmbeddedColorLine($"{GetStepLabel(step)}: {ColorBool(success)}");
    }

    private string BuildStepError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return CliMessages.HandshakeStepFailed;
        }

        if (error.Contains("Incorrect API key or IP address", StringComparison.OrdinalIgnoreCase))
        {
            return CliMessages.BuildApiKeyIpHint(
                $"{error} Verify API key and environment, and leave permitted IP empty unless needed.",
                _baseUrl,
                _permittedIp);
        }

        return error;
    }
    
}
