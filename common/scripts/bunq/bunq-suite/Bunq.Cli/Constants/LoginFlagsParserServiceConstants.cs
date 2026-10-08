namespace Bunq.Cli.Constants;

public static class LoginFlagsParserServiceConstants
{
    public const string ErrorCodeUnknownFlag = "login-flags.unknown-flag";
    public const string ErrorCodeMissingValue = "login-flags.missing-value";
    public const string ErrorCodeInvalidBaseUrl = "login-flags.invalid-base-url";
    public const string ErrorCodeHttpNotAllowed = "login-flags.http-not-allowed";
    public const string ErrorCodeHttpsRequired = "login-flags.https-required";

    public static string BuildInvalidUrlMessage(string flagName, string url) =>
        $"Invalid URL for {flagName}: {url}";

    public static string BuildUnknownFlagMessage(string flag) =>
        $"Unknown flag: {flag}";

    public static string BuildMissingValueMessage(string flagName) =>
        $"Missing value for {flagName}";

    public static string BuildHttpSchemeNotAllowedMessage(string flagName) =>
        $"{flagName} must use https://. http:// is not allowed.";

    public static string BuildHttpsUrlRequiredMessage(string flagName) =>
        $"{flagName} must use an https:// URL.";
}
