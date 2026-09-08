namespace bunq_tools.Commands;

public class LoginCommand
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Logging in to Bunq...");
    }
}
