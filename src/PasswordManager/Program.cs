using System.Security.Cryptography;

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

    static string GetPassword(string prompt = "Enter master password: ")
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

        byte[] secretKeyBytes = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(secretKeyBytes);
        }
        string secretKey = Convert.ToBase64String(secretKeyBytes);

        Console.WriteLine("Secret key: " + secretKey);

        byte[] vaultKey = new Rfc2898DeriveBytes(
            masterPassword, secretKeyBytes, 10000, HashAlgorithmName.SHA256
        ).GetBytes(16);

        Client client = new(secretKey);
        client.WriteToFile(clientPath);

        Aes aesAlg = Aes.Create();
        aesAlg.Key = vaultKey;
        string IV = Convert.ToBase64String(aesAlg.IV);

        Server server = new(IV);
        server.WriteToFile(serverPath, vaultKey);
    }

    static void Set(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine(
                "Not enough arguments. Usage: set <client> <server> <prop>"
            );
            return;
        }

        bool shouldGenerate = false;
        if (args.Length > 3)
        {
            shouldGenerate = args[3] == "-g" || args[3] == "--generate";
        }

        string masterPassword = GetPassword("Enter master password: ");

        string password = "";
        if (shouldGenerate)
        {
            password = "pnasfasnifa";
        }
        else
        {
            password = GetPassword("Enter password: ");
        }

        string clientPath = args[0];
        string serverPath = args[1];
        string prop = args[2];

        Client client = Client.ReadFromFile(clientPath);

        byte[] vaultKey = new Rfc2898DeriveBytes(
            masterPassword, Convert.FromBase64String(client.SecretKey), 10000, HashAlgorithmName.SHA256
        ).GetBytes(16);

        Server server = Server.ReadFromFile(serverPath, vaultKey);

        Aes aesAlg = Aes.Create();
        aesAlg.Key = vaultKey;
        aesAlg.IV = Convert.FromBase64String(server.IV);

        server.Vault.Set(prop, password);

        server.WriteToFile(serverPath, vaultKey);
    }
}