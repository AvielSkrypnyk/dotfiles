using Bunq.Cli.Helpers;
using Bunq.Core.Interfaces;

namespace Bunq.Cli.Commands;

public sealed class ContextPathCommand
{
    private readonly IBunqContextRepository _contextRepository;

    public ContextPathCommand(IBunqContextRepository contextRepository)
    {
        _contextRepository = contextRepository;
    }

    public Task ExecuteAsync()
    {
        ColorConsole.WriteInfo(_contextRepository.GetFilePath());
        return Task.CompletedTask;
    }
}
