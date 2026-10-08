using Bunq.Cli.Helpers;
using Bunq.Core.Interfaces;

namespace Bunq.Cli.Commands;

public sealed class ResetCommand(IBunqContextRepository contextRepository)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await contextRepository.DeleteAsync(cancellationToken);
        ColorConsole.WriteSuccess("bunq context removed.");
    }
}
