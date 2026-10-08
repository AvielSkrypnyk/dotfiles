using Bunq.Cli.Constants;
using Bunq.Cli.Models;
using Bunq.Core.Common;

namespace Bunq.Cli.Services;

/// <summary>
/// Parses the flags typed after <c>bunq login</c> into <see cref="LoginOptions"/>.
/// Example: <c>--sandbox --api-key abc123</c> becomes a sandbox login with API key "abc123".
/// </summary>
public sealed class LoginFlagsParserService
{
    public OperationResult<LoginOptions> Parse(string[] flagsAndValues)
    {
        var loginOptions = new LoginOptions();

        // A queue hands out the words one by one, so a flag can take the next word as its value.
        var remainingWords = new Queue<string>(flagsAndValues);

        while (remainingWords.TryDequeue(out var flag))
        {
            var flagResult = ApplyFlag(flag, remainingWords, loginOptions);
            if (!flagResult.Success)
            {
                return OperationResult<LoginOptions>.Fail(flagResult.MessageType, flagResult.Code!, flagResult.Error!);
            }
        }

        loginOptions.BaseUrl = EnsureTrailingSlash(loginOptions.BaseUrl);
        return OperationResult<LoginOptions>.Ok(loginOptions);
    }

    private static OperationResult ApplyFlag(string flag, Queue<string> remainingWords, LoginOptions loginOptions)
    {
        var flagName = flag.ToLowerInvariant();

        switch (flagName)
        {
            // Flags without a value
            case CliConstants.OptionSandbox:
                loginOptions.BaseUrl = CliConstants.BaseUrlSandbox;
                return OperationResult.Ok();

            case CliConstants.OptionProduction:
                loginOptions.BaseUrl = CliConstants.BaseUrlProduction;
                return OperationResult.Ok();

            case CliConstants.OptionForce:
                loginOptions.ForceRecreateContext = true;
                return OperationResult.Ok();

            // Flags with a value: the next word is the value
            case CliConstants.OptionApiKey:
                if (!remainingWords.TryDequeue(out var apiKey))
                {
                    return MissingValueFailure(flagName);
                }

                loginOptions.ApiKey = apiKey;
                return OperationResult.Ok();

            case CliConstants.OptionDescription:
                if (!remainingWords.TryDequeue(out var deviceDescription))
                {
                    return MissingValueFailure(flagName);
                }

                loginOptions.DeviceDescription = deviceDescription;
                return OperationResult.Ok();

            case CliConstants.OptionPermittedIp:
                if (!remainingWords.TryDequeue(out var permittedIp))
                {
                    return MissingValueFailure(flagName);
                }

                loginOptions.PermittedIps.Add(permittedIp);
                return OperationResult.Ok();

            case CliConstants.OptionBaseUrl:
                if (!remainingWords.TryDequeue(out var baseUrl))
                {
                    return MissingValueFailure(flagName);
                }

                return ApplyBaseUrl(baseUrl, loginOptions);

            default:
                return Failure(
                    LoginFlagsParserServiceConstants.ErrorCodeUnknownFlag,
                    LoginFlagsParserServiceConstants.BuildUnknownFlagMessage(flag));
        }
    }

    private static OperationResult ApplyBaseUrl(string baseUrl, LoginOptions loginOptions)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var parsedUrl))
        {
            return Failure(
                LoginFlagsParserServiceConstants.ErrorCodeInvalidBaseUrl,
                LoginFlagsParserServiceConstants.BuildInvalidUrlMessage(CliConstants.OptionBaseUrl, baseUrl));
        }

        if (parsedUrl.Scheme == Uri.UriSchemeHttp)
        {
            return Failure(
                LoginFlagsParserServiceConstants.ErrorCodeHttpNotAllowed,
                LoginFlagsParserServiceConstants.BuildHttpSchemeNotAllowedMessage(CliConstants.OptionBaseUrl));
        }

        if (parsedUrl.Scheme != Uri.UriSchemeHttps)
        {
            return Failure(
                LoginFlagsParserServiceConstants.ErrorCodeHttpsRequired,
                LoginFlagsParserServiceConstants.BuildHttpsUrlRequiredMessage(CliConstants.OptionBaseUrl));
        }

        loginOptions.BaseUrl = parsedUrl.AbsoluteUri;
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
