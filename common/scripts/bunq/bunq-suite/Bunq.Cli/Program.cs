using bunq_tools.Commands;
using Bunq.Core.Interfaces;
using Bunq.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IContextStore, JsonContextStore>();
services.AddTransient<LoginCommand>();

var serviceProvider = services.BuildServiceProvider();

var loginCmd = serviceProvider.GetRequiredService<LoginCommand>();
await loginCmd.ExecuteAsync();
