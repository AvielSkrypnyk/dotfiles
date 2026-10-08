using Bunq.Cli.Commands;
using Bunq.Cli.Constants;
using Bunq.Cli.Helpers;
using Bunq.Cli.Services;
using Bunq.Core.Interfaces;
using Bunq.Core.Services;
using Bunq.Infrastructure.Services.BunqApi;
using Bunq.Infrastructure.Services.Persistence;
using Bunq.Infrastructure.Services.Security;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IOperatingSystemService, OperatingSystemService>();
services.AddSingleton<IContextFilePermissionStrategy, WindowsContextFilePermissionStrategy>();
services.AddSingleton<IContextFilePermissionStrategy, UnixContextFilePermissionStrategy>();
services.AddSingleton<IContextFileSecurityService, ContextFileSecurityService>();
services.AddSingleton<IBunqContextRepository, JsonFileBunqContextRepository>();
services.AddSingleton<IRsaService, RsaService>();
services.AddSingleton<IBunqApiService, BunqApiService>();
services.AddSingleton<LoginFlagsParserService>();
services.AddSingleton<ApiKeyProviderService>();
services.AddSingleton<BunqLoginService>();
services.AddTransient<LoginCommand>();
services.AddTransient<ResetCommand>();
services.AddTransient<ContextPathCommand>();
services.AddTransient<MenuCommand>();

var serviceProvider = services.BuildServiceProvider();

if (args.Length == 0)
{
    var menu = serviceProvider.GetRequiredService<MenuCommand>();
    await menu.ExecuteAsync();
    return;
}

var command = args[0].ToLowerInvariant();
var commandArgs = args.Skip(1).ToArray();


switch (command)
{
    case CliConstants.CommandLogin:
        var loginCmd = serviceProvider.GetRequiredService<LoginCommand>();
        await loginCmd.ExecuteAsync(commandArgs);
        break;
    case CliConstants.CommandReset:
        var resetCmd = serviceProvider.GetRequiredService<ResetCommand>();
        await resetCmd.ExecuteAsync();
        break;
    case CliConstants.CommandContextPath:
        var contextPathCmd = serviceProvider.GetRequiredService<ContextPathCommand>();
        await contextPathCmd.ExecuteAsync();
        break;
    case CliConstants.CommandMenu:
        var menuCmd = serviceProvider.GetRequiredService<MenuCommand>();
        await menuCmd.ExecuteAsync();
        break;
    default:
        PrintUsage();
        Environment.ExitCode = 1;
        break;
}

static void PrintUsage()
{
    ColorConsole.WriteInfo("bunq <command> [flags]");
    ColorConsole.WriteLine(string.Empty);
    ColorConsole.WriteInfo("Commands:");
    ColorConsole.WriteLine($"  {CliConstants.CommandLogin}    Create installation, register device, and create session");
    ColorConsole.WriteLine($"  {CliConstants.CommandReset}    Remove locally stored bunq context");
    ColorConsole.WriteLine($"  {CliConstants.CommandContextPath}  Print the location of context.json");
    ColorConsole.WriteLine($"  {CliConstants.CommandMenu}     Open the interactive setup menu");
    ColorConsole.WriteLine(string.Empty);
    ColorConsole.WriteInfo("Login flags:");
    ColorConsole.WriteLine($"  {CliConstants.FlagApiKey} <value>   API key (or set {CliConstants.EnvVarApiKey} env var)");
    ColorConsole.WriteLine($"  {CliConstants.FlagSandbox}           Use sandbox API ({CliConstants.BaseUrlSandbox})");
    ColorConsole.WriteLine($"  {CliConstants.FlagProduction}        Use production API ({CliConstants.BaseUrlProduction})");
    ColorConsole.WriteLine($"  {CliConstants.FlagBaseUrl} <url>    Override base API URL (https:// only)");
    ColorConsole.WriteLine($"  {CliConstants.FlagDescription} <txt> Device description sent to bunq");
    ColorConsole.WriteLine($"  {CliConstants.FlagPermittedIp} <ip> Permitted IP for device registration (repeatable, leave empty to use current IP)");
    ColorConsole.WriteLine($"  {CliConstants.FlagForce}             Re-create context even if one already exists");
    ColorConsole.WriteLine(string.Empty);
    ColorConsole.WriteInfo("Environment variables:");
    ColorConsole.WriteLine($"  {CliConstants.EnvVarApiKey}        API key fallback for login");
    ColorConsole.WriteLine($"  {CliConstants.EnvVarContextFile}   Optional override for context.json path");
}
