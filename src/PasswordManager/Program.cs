using System.Security.Cryptography;

namespace PasswordManager;

public static class Program
{
    static Dictionary<string, Action<string[]>> Commands { get; } = new()
    {
        { "init", Init }
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

    static string GetMasterPassword()
    {
        while (true)
        {
            Console.WriteLine("Enter master password: ");
            string? masterPassword = Console.ReadLine();

            if (string.IsNullOrEmpty(masterPassword))
            {
                Console.WriteLine("No password provided.");
                continue;
            }

            return masterPassword;
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

        string masterPassword = GetMasterPassword();

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
        aesAlg.GenerateIV();
        string IV = Convert.ToBase64String(aesAlg.IV);

        Server server = new(IV);
        server.WriteToFile(serverPath);
    }
}