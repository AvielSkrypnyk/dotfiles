namespace Bunq.Cli.Constants;

public static class CliMessages
{
    public const string MenuTitle = "bunq setup menu";
    public const string InvalidMenuChoice = "Unknown option. Choose a number from the menu.";
    public const string Canceled = "Canceled.";
    public const string ApiKeyPrompt = "Paste bunq API key: ";
    public const string ApiKeyNotChanged = "API key not changed.";
    public const string ApiKeySaved = "API key saved for this run.";
    public const string ApiKeyFormatChecked = "API key format looks valid. Real validation happens during device registration.";
    public const string ApiKeyRequired = "API key is required before this step.";
    public const string ContextMissing = "No context found yet. Run setup first.";
    public const string SelectionPrompt = "Select option and press Enter: ";
    public const string EnvironmentPrompt = "Environment [sandbox/production] (default: sandbox): ";
    public static string DeviceDescriptionPrompt(string defaultDescription) =>
        $"Device description (default: {defaultDescription}): ";
    public const string PermittedIpPrompt = "Permitted IP (default: current IP, use * only if key allows it): ";
    public const string HandshakeStepFailed = "Handshake step failed.";

    public static string BuildApiKeyInvalidMessage(int minLength) =>
        $"API key looks invalid (min {minLength} chars, only letters, numbers, - and _). It was not saved.";

    public static string BuildApiKeyIpHint(string error, string baseUrl, string permittedIp)
    {
        var ipValue = string.IsNullOrWhiteSpace(permittedIp) ? "current IP (default)" : permittedIp;
        return $"{error} Environment: {baseUrl} | Permitted IP: {ipValue}";
    }
}
