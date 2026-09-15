namespace Bunq.Cli.Constants;

public static class CliConstants
{
    public const string CommandLogin = "login";
    public const string CommandReset = "reset";
    public const string CommandContextPath = "context-path";
    public const string CommandMenu = "menu";

    public const string OptionApiKey = "--api-key";
    public const string OptionSandbox = "--sandbox";
    public const string OptionProduction = "--production";
    public const string OptionBaseUrl = "--base-url";
    public const string OptionDescription = "--description";
    public const string OptionPermittedIp = "--permitted-ip";
    public const string OptionForce = "--force";

    public const string EnvApiKey = "BUNQ_API_KEY";
    public const string EnvContextFile = "BUNQ_CONTEXT_FILE";

    public const string BaseUrlProduction = "https://api.bunq.com/";
    public const string BaseUrlSandbox = "https://public-api.sandbox.bunq.com/";
    public const string DefaultDeviceDescription = "bunq-csharp-cli";
    public const string WildcardPermittedIp = "*";
    public const int MinApiKeyLength = 10;
    public const int DefaultRequestTimeoutSeconds = 30;
}
