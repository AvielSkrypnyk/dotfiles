using Bunq.Cli.Helpers;
using Bunq.Core.Interfaces;

namespace Bunq.Cli.Commands;

public sealed class ContextPathCommand
{
    private readonly IBunqContextRepository _contextStore;

    public ContextPathCommand(IBunqContextRepository contextStore)
    {
        _contextStore = contextStore;
    }

    public Task ExecuteAsync()
    {
        ColorConsole.WriteInfo(_contextStore.GetPath());
        return Task.CompletedTask;
    }
}
