using Bunq.Cli.Constants;
using Bunq.Cli.Models;

namespace Bunq.Cli.Services;

public sealed class LoginOptionsParserService
{
    public bool TryParse(string[] args, out LoginOptions options, out string? error)
    {
        options = new LoginOptions();
        error = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLowerInvariant();
            switch (arg)
            {
                case CliConstants.OptionApiKey:
                    if (!TryReadValue(args, ref i, CliConstants.OptionApiKey, out var apiKey, out error))
                    {
                        return false;
                    }

                    options.ApiKey = apiKey;
                    break;

                case CliConstants.OptionSandbox:
                    options.BaseUrl = CliConstants.BaseUrlSandbox;
                    break;

                case CliConstants.OptionProduction:
                    options.BaseUrl = CliConstants.BaseUrlProduction;
                    break;

                case CliConstants.OptionBaseUrl:
                    if (!TryReadValue(args, ref i, CliConstants.OptionBaseUrl, out var baseUrl, out error))
                    {
                        return false;
                    }

                    options.BaseUrl = NormalizeBaseUrl(baseUrl);
                    break;

                case CliConstants.OptionDescription:
                    if (!TryReadValue(args, ref i, CliConstants.OptionDescription, out var description, out error))
                    {
                        return false;
                    }

                    options.DeviceDescription = description;
                    break;

                case CliConstants.OptionPermittedIp:
                    if (!TryReadValue(args, ref i, CliConstants.OptionPermittedIp, out var permittedIp, out error))
                    {
                        return false;
                    }

                    options.PermittedIps.Add(permittedIp);
                    break;

                case CliConstants.OptionForce:
                    options.Force = true;
                    break;

                default:
                    error = $"Unknown option: {args[i]}";
                    return false;
            }
        }

        options.BaseUrl = NormalizeBaseUrl(options.BaseUrl);
        return true;
    }

    private static bool TryReadValue(
        string[] args,
        ref int index,
        string optionName,
        out string value,
        out string? error)
    {
        value = string.Empty;
        error = null;

        if (index + 1 >= args.Length)
        {
            error = $"Missing value for {optionName}";
            return false;
        }

        index++;
        value = args[index];
        return true;
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        return baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";
    }
}
