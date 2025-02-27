namespace PasswordManager;

class Program
{
    static Dictionary<string, Action<string[]>> Commands { get; } = new()
    {
    };

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No command provided");
            return;
        }

        string commandName = args[0];
        string[] commandArgs = args.Skip(1).ToArray();

        if (!Commands.TryGetValue(commandName, out Action<string[]>? command))
        {
            Console.WriteLine($"Unknown command: {commandName}.");
            return;
        }

        command(commandArgs);
    }
}