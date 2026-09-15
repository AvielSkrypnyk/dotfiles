using Bunq.Cli.Helpers;
using Bunq.Core.Interfaces;

namespace Bunq.Cli.Commands;

public sealed class ResetCommand(IBunqContextRepository contextStore)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await contextStore.DeleteAsync(cancellationToken);
        ColorConsole.WriteSuccess("bunq context removed.");
    }
}
