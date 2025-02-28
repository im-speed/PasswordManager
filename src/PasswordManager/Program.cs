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

        byte[] secretKey = new byte[16];
        byte[] IV = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(secretKey);
            rng.GetBytes(IV);
        }

        Console.WriteLine("Secret key: " + Convert.ToBase64String(secretKey));

        byte[] vaultKey = new Rfc2898DeriveBytes(
            masterPassword, secretKey, 10000, HashAlgorithmName.SHA256
        ).GetBytes(16);

        Console.WriteLine(Convert.ToBase64String(vaultKey));

        Client client = new(Convert.ToBase64String(secretKey));
        client.WriteToFile(clientPath);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = vaultKey;
            aesAlg.IV = IV;
        }

        Console.WriteLine(Convert.ToBase64String(IV));
    }
}