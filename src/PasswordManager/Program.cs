using PasswordManager.Keys;

namespace PasswordManager;

public static class Program
{
    static Dictionary<string, Action<string[]>> Commands { get; } = new()
    {
        { "init", Init },
        { "set", Set },
    };

    public static void Main(string[] args)
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

    /// <summary>
    /// Prompts the user for a password and returns it.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading the password.</param>
    static string GetPassword(string prompt = "Enter password: ")
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string? password = Console.ReadLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("No password provided.");
                continue;
            }

            return password;
        }
    }

    static void Init(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Not enough arguments. Usage: init <client> <server>");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];

        string masterPassword = GetPassword("Enter master password: ");

        SecretKey secretKey = new();

        Console.WriteLine("Your secret key: " + secretKey);

        VaultKey vaultKey = new(masterPassword, secretKey);

        Client client = new()
        {
            SecretKey = secretKey
        };
        client.WriteToFile(clientPath);

        Server server = new();
        server.WriteToFile(serverPath, vaultKey);
    }

    static void Set(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Not enough arguments. Usage: set <client> <server> <prop>");
            return;
        }

        bool shouldGenerate = false;
        if (args.Length > 3)
        {
            shouldGenerate = args[3] == "-g" || args[3] == "--generate";
        }

        string masterPassword = GetPassword("Enter master password: ");

        string password;
        if (shouldGenerate)
        {
            // TODO: Generate password
            password = "pnasfasnifa";
        }
        else
        {
            password = GetPassword();
        }

        string clientPath = args[0];
        string serverPath = args[1];
        string prop = args[2];

        Client? client = Client.ReadFromFile(clientPath);
        if (client == null) return;

        VaultKey vaultKey = new(masterPassword, client.SecretKey);
        Server? server = Server.ReadFromFile(serverPath, vaultKey);
        if (server == null) return;

        server.Vault.Set(prop, password);
        server.WriteToFile(serverPath, vaultKey);
    }
}