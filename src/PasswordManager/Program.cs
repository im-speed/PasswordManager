using System.Security.Cryptography;
using PasswordManager.Keys;

namespace PasswordManager;

public static class Program
{
    static readonly string alphaNumerics = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    static Dictionary<string, Action<string[]>> Commands { get; } = new()
    {
        { "init", Init },
        { "create", Create },
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
    /// <param name="nullWarning">The text to display if the password is null.</param>
    static string GetPassword(
        string prompt,
        string nullWarning = "No password provided."
    )
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string? password = Console.ReadLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine(nullWarning);
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

        string masterPassword = GetPassword("Enter new master password: ");

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

    static void Create(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Not enough arguments. Usage: create <client> <server>");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];

        string masterPassword = GetPassword("Enter your master password: ");
        string secretKeyInput = GetPassword("Enter your secret key: ", "No key provided.");

        SecretKey secretKey = new(Convert.FromBase64String(secretKeyInput));
        VaultKey vaultKey = new(masterPassword, secretKey);
        Server? server = Server.ReadFromFile(serverPath, vaultKey);
        if (server == null) return;

        Client client = new()
        {
            SecretKey = secretKey
        };
        client.WriteToFile(clientPath);
    }

    static void Set(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Not enough arguments. Usage: set <client> <server> <prop>");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];
        string prop = args[2];

        bool shouldGenerate = false;
        if (args.Length > 3)
        {
            shouldGenerate = args[3] == "-g" || args[3] == "--generate";
        }

        string masterPassword = GetPassword("Enter your master password: ");

        string password;
        if (shouldGenerate)
        {
            password = RandomNumberGenerator.GetString(alphaNumerics, 20);
            Console.WriteLine($"Generated password for {prop}: {password}");
        }
        else
        {
            password = GetPassword($"Enter new password for {prop}: ");
        }

        Client? client = Client.ReadFromFile(clientPath);
        if (client == null) return;

        VaultKey vaultKey = new(masterPassword, client.SecretKey);
        Server? server = Server.ReadFromFile(serverPath, vaultKey);
        if (server == null) return;

        server.Vault.Set(prop, password);
        server.WriteToFile(serverPath, vaultKey);
    }
}