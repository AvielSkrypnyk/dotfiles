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

services.AddSingleton<IPlatformService, RuntimePlatformService>();
services.AddSingleton<IContextFilePermissionStrategy, WindowsContextFilePermissionStrategy>();
services.AddSingleton<IContextFilePermissionStrategy, UnixContextFilePermissionStrategy>();
services.AddSingleton<IContextFileSecurityService, ContextFileSecurityService>();
services.AddSingleton<IBunqContextRepository, JsonBunqContextRepositoryService>();
services.AddSingleton<IRsaService, RsaService>();
services.AddSingleton<IBunqApiService, BunqApiService>();
services.AddSingleton<LoginFlagsParserService>();
services.AddSingleton<ApiKeyResolverService>();
services.AddSingleton<BunqAuthService>();
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
    ColorConsole.WriteInfo("bunq <command> [options]");
    ColorConsole.WriteLine(string.Empty);
    ColorConsole.WriteInfo("Commands:");
    ColorConsole.WriteLine($"  {CliConstants.CommandLogin}    Create installation, register device, and create session");
    ColorConsole.WriteLine($"  {CliConstants.CommandReset}    Remove locally stored bunq context");
    ColorConsole.WriteLine($"  {CliConstants.CommandContextPath}  Print the location of context.json");
    ColorConsole.WriteLine($"  {CliConstants.CommandMenu}     Open the interactive setup menu");
    ColorConsole.WriteLine(string.Empty);
    ColorConsole.WriteInfo("Login options:");
    ColorConsole.WriteLine($"  {CliConstants.OptionApiKey} <value>   API key (or set {CliConstants.EnvApiKey} env var)");
    ColorConsole.WriteLine($"  {CliConstants.OptionSandbox}           Use sandbox API ({CliConstants.BaseUrlSandbox})");
    ColorConsole.WriteLine($"  {CliConstants.OptionProduction}        Use production API ({CliConstants.BaseUrlProduction})");
    ColorConsole.WriteLine($"  {CliConstants.OptionBaseUrl} <url>    Override base API URL (https:// only)");
    ColorConsole.WriteLine($"  {CliConstants.OptionDescription} <txt> Device description sent to bunq");
    ColorConsole.WriteLine($"  {CliConstants.OptionPermittedIp} <ip> Permitted IP for device registration (repeatable, leave empty to use current IP)");
    ColorConsole.WriteLine($"  {CliConstants.OptionForce}             Re-create context even if one already exists");
    ColorConsole.WriteLine(string.Empty);
    ColorConsole.WriteInfo("Environment variables:");
    ColorConsole.WriteLine($"  {CliConstants.EnvApiKey}        API key fallback for login");
    ColorConsole.WriteLine($"  {CliConstants.EnvContextFile}   Optional override for context.json path");
}
