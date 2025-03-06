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
        { "get", Get },
        { "delete", Delete },
        { "secret", Secret },
        { "change", Change },
    };

    public static bool IsBase64Valid(string base64String)
    {
        // We create a span of bytes with the same length as the input string
        Span<byte> buffer = new(new byte[base64String.Length]);

        // We attempt to parse the Base64 string into bytes using TryFromBase64String
        // This method returns true if successful and false otherwise
        bool isValid = Convert.TryFromBase64String(base64String, buffer, out _);

        // We return the result indicating whether the string is a valid Base64 encoding
        return isValid;
    }

    public static void Main(string[] args)
    {
        args = ["create", "client.json", "server.json"];

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
            Console.WriteLine("Incorrect amount of arguments. Usage: init <client> <server>");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];

        string masterPassword = GetPassword("Enter new master password: ");

        SecretKey secretKey = new();

        Console.WriteLine(secretKey);
        Console.WriteLine("↑ Your secret key ↑");

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
            Console.WriteLine("Incorrect amount of arguments. Usage: create <client> <server>");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];

        string masterPassword = GetPassword("Enter your master password: ");
        string secretKeyInput = GetPassword("Enter your secret key: ", "No key provided.");

        SecretKey? secretKey = SecretKey.FromString(secretKeyInput);
        if (secretKey == null) return;

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
        if (args.Length < 3 || args.Length > 4)
        {
            Console.WriteLine("Incorrect amount of arguments. Usage: set <client> <server> <prop> [-g]");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];
        string prop = args[2];

        bool shouldGenerate = false;
        if (args.Length == 4)
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

    static void Get(string[] args)
    {
        if (args.Length < 2 || args.Length > 3)
        {
            Console.WriteLine("Incorrect amount of arguments. Usage: get <client> <server> [<prop>]");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];
        string? prop = args.Length == 3 ? args[2] : null;

        string masterPassword = GetPassword("Enter your master password: ");

        Client? client = Client.ReadFromFile(clientPath);
        if (client == null) return;

        VaultKey vaultKey = new(masterPassword, client.SecretKey);
        Server? server = Server.ReadFromFile(serverPath, vaultKey);
        if (server == null) return;

        if (prop != null)
        {
            string? password = server.Vault.Get(prop);
            if (password != null)
            {
                Console.WriteLine($"Password for {prop}: ");
                Console.WriteLine(password);
            }
        }
        else
        {
            SortedSet<string> properties = new(server.Vault.Values.Keys);
            Console.WriteLine("Properties: ");
            Console.WriteLine(string.Join("\n", properties));
        }
    }

    static void Delete(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Incorrect amount of arguments. Usage: delete <client> <server> <prop>");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];
        string prop = args[2];

        string masterPassword = GetPassword("Enter your master password: ");

        Client? client = Client.ReadFromFile(clientPath);
        if (client == null) return;

        VaultKey vaultKey = new(masterPassword, client.SecretKey);
        Server? server = Server.ReadFromFile(serverPath, vaultKey);
        if (server == null) return;

        if (server.Vault.Values.Remove(prop))
        {
            server.WriteToFile(serverPath, vaultKey);
            Console.WriteLine($"Deleted property '{prop}'.");
        }
        else
        {
            Console.WriteLine($"The property '{prop}' does not exist.");
        }
    }

    static void Secret(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Incorrect amount of arguments. Usage: secret <client>");
            return;
        }

        Client? client = Client.ReadFromFile(args[0]);
        if (client == null) return;

        Console.WriteLine(client.SecretKey);
        Console.WriteLine("↑ Your secret key ↑");
    }

    static void Change(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Incorrect amount of arguments. Usage: change <client> <server>");
            return;
        }

        string clientPath = args[0];
        string serverPath = args[1];

        string masterPassword = GetPassword("Enter your master password: ");

        Client? client = Client.ReadFromFile(clientPath);
        if (client == null) return;

        VaultKey vaultKey = new(masterPassword, client.SecretKey);
        Server? server = Server.ReadFromFile(serverPath, vaultKey);
        if (server == null) return;

        string newPassword = GetPassword("Enter new master password: ");
        VaultKey newVaultKey = new(newPassword, client.SecretKey);

        server.WriteToFile(serverPath, newVaultKey);
    }
}