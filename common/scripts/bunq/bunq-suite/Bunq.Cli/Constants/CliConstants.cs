namespace Bunq.Cli.Constants;

public static class CliConstants
{
    public const string CommandLogin = "login";
    public const string CommandReset = "reset";
    public const string CommandContextPath = "context-path";
    public const string CommandMenu = "menu";

    public const string FlagApiKey = "--api-key";
    public const string FlagSandbox = "--sandbox";
    public const string FlagProduction = "--production";
    public const string FlagBaseUrl = "--base-url";
    public const string FlagDescription = "--description";
    public const string FlagPermittedIp = "--permitted-ip";
    public const string FlagForce = "--force";

    public const string EnvVarApiKey = "BUNQ_API_KEY";
    public const string EnvVarContextFile = "BUNQ_CONTEXT_FILE";

    public const string BaseUrlProduction = "https://api.bunq.com/";
    public const string BaseUrlSandbox = "https://public-api.sandbox.bunq.com/";
    public const string DefaultDeviceDescription = "bunq-csharp-cli";
    public const string WildcardPermittedIp = "*";
    public const int MinApiKeyLength = 10;
    public const int DefaultRequestTimeoutSeconds = 30;
}
