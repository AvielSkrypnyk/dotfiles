namespace Bunq.Cli.Helpers;


public static class MaskedInputReader
{
    public static string ReadSecret()
    {
        var chars = new List<char>();
        
        var ignoredKeys = new List<ConsoleKey>
        {
            ConsoleKey.Escape, ConsoleKey.DownArrow, ConsoleKey.UpArrow, ConsoleKey.Spacebar, ConsoleKey.LeftArrow,
            ConsoleKey.RightArrow
        };

        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);
            
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    return new string(chars.ToArray());
                case ConsoleKey.Backspace:
                    if (chars.Count > 0)
                    {
                        chars.RemoveAt(chars.Count - 1);
                        Console.Write("\b \b");
                    }
                    break;
                case var consoleKey when ignoredKeys.Contains(consoleKey):
                    break;
                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        chars.Add(keyInfo.KeyChar);
                        Console.Write("*");    
                    }
                    break;
            }
        }
    }
}
