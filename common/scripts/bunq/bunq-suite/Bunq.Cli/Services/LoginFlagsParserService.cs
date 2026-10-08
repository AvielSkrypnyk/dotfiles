using Bunq.Cli.Constants;
using Bunq.Cli.Models;
using Bunq.Core.Common;

namespace Bunq.Cli.Services;

/// <summary>
/// Parses the flags typed after <c>bunq login</c> into <see cref="LoginSettings"/>.
/// Example: <c>--sandbox --api-key abc123</c> becomes a sandbox login with API key "abc123".
/// </summary>
public sealed class LoginFlagsParserService
{
    public OperationResult<LoginSettings> Parse(string[] flagsAndValues)
    {
        var loginSettings = new LoginSettings();

        // A queue hands out the words one by one, so a flag can take the next word as its value.
        var remainingWords = new Queue<string>(flagsAndValues);

        while (remainingWords.TryDequeue(out var flag))
        {
            var flagResult = ApplyFlag(flag, remainingWords, loginSettings);
            if (!flagResult.Success)
            {
                return OperationResult<LoginSettings>.Fail(flagResult.MessageType, flagResult.Code!, flagResult.Error!);
            }
        }

        loginSettings.BaseUrl = EnsureTrailingSlash(loginSettings.BaseUrl);
        return OperationResult<LoginSettings>.Ok(loginSettings);
    }

    private static OperationResult ApplyFlag(string flag, Queue<string> remainingWords, LoginSettings loginSettings)
    {
        var flagName = flag.ToLowerInvariant();

        switch (flagName)
        {
            // Flags without a value
            case CliConstants.FlagSandbox:
                loginSettings.BaseUrl = CliConstants.BaseUrlSandbox;
                return OperationResult.Ok();

            case CliConstants.FlagProduction:
                loginSettings.BaseUrl = CliConstants.BaseUrlProduction;
                return OperationResult.Ok();

            case CliConstants.FlagForce:
                loginSettings.ForceRecreateContext = true;
                return OperationResult.Ok();

            // Flags with a value: the next word is the value
            case CliConstants.FlagApiKey:
                if (!remainingWords.TryDequeue(out var apiKey))
                {
                    return MissingValueFailure(flagName);
                }

                loginSettings.ApiKey = apiKey;
                return OperationResult.Ok();

            case CliConstants.FlagDescription:
                if (!remainingWords.TryDequeue(out var deviceDescription))
                {
                    return MissingValueFailure(flagName);
                }

                loginSettings.DeviceDescription = deviceDescription;
                return OperationResult.Ok();

            case CliConstants.FlagPermittedIp:
                if (!remainingWords.TryDequeue(out var permittedIp))
                {
                    return MissingValueFailure(flagName);
                }

                loginSettings.PermittedIps.Add(permittedIp);
                return OperationResult.Ok();

            case CliConstants.FlagBaseUrl:
                if (!remainingWords.TryDequeue(out var baseUrl))
                {
                    return MissingValueFailure(flagName);
                }

                return ApplyBaseUrl(baseUrl, loginSettings);

            default:
                return Failure(
                    LoginFlagsParserServiceConstants.ErrorCodeUnknownFlag,
                    LoginFlagsParserServiceConstants.BuildUnknownFlagMessage(flag));
        }
    }

    private static OperationResult ApplyBaseUrl(string baseUrl, LoginSettings loginSettings)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var parsedUrl))
        {
            return Failure(
                LoginFlagsParserServiceConstants.ErrorCodeInvalidBaseUrl,
                LoginFlagsParserServiceConstants.BuildInvalidUrlMessage(CliConstants.FlagBaseUrl, baseUrl));
        }

        if (parsedUrl.Scheme == Uri.UriSchemeHttp)
        {
            return Failure(
                LoginFlagsParserServiceConstants.ErrorCodeHttpNotAllowed,
                LoginFlagsParserServiceConstants.BuildHttpSchemeNotAllowedMessage(CliConstants.FlagBaseUrl));
        }

        if (parsedUrl.Scheme != Uri.UriSchemeHttps)
        {
            return Failure(
                LoginFlagsParserServiceConstants.ErrorCodeHttpsRequired,
                LoginFlagsParserServiceConstants.BuildHttpsUrlRequiredMessage(CliConstants.FlagBaseUrl));
        }

        loginSettings.BaseUrl = parsedUrl.AbsoluteUri;
        return OperationResult.Ok();
    }

    private static string EnsureTrailingSlash(string url)
    {
        return url.EndsWith('/') ? url : $"{url}/";
    }

    private static OperationResult MissingValueFailure(string flagName)
    {
        return Failure(
            LoginFlagsParserServiceConstants.ErrorCodeMissingValue,
            LoginFlagsParserServiceConstants.BuildMissingValueMessage(flagName));
    }

    private static OperationResult Failure(string errorCode, string errorMessage)
    {
        return OperationResult.Fail(MessageTypeEnum.BrokenBusinessRule, errorCode, errorMessage);
    }
}
