using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PasswordManager.JsonClasses;
using PasswordManager.Keys;

namespace PasswordManager;

public class Server()
{
    public byte[] IV { get; set; } = Aes.Create().IV;

    public Vault Vault { get; set; } = new();

    /// <summary>
    /// Creates a server by reading it from a json file.
    /// </summary>
    /// <param name="vaultKey">The key used to decrypt the vault.</param>
    public static Server? ReadFromFile(string path, VaultKey vaultKey)
    {
        JsonServer jsonServer;
        try
        {
            jsonServer = JsonSerializer.Deserialize<JsonServer>(File.ReadAllText(path))!;
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to read server from file. Try running 'init'.");
            return null;
        }


        byte[] IV = Convert.FromBase64String(jsonServer.IV);
        byte[] encryptedVault = Convert.FromBase64String(jsonServer.Vault);

        Vault? vault = Vault.Decrypt(encryptedVault, vaultKey, IV);
        if (vault == null) return null;

        return new()
        {
            IV = IV,
            Vault = vault
        };
    }

    /// <summary>
    /// Writes the server to a json file, overwriting it if it already exists.
    /// </summary>
    /// <param name="vaultKey">The key used to encrypt the vault.</param>
    public void WriteToFile(string path, VaultKey vaultKey)
    {
        JsonServer jsonServer = new()
        {
            IV = Convert.ToBase64String(IV),
            Vault = Vault.Encrypt(vaultKey, IV)
        };

        File.WriteAllText(path, JsonSerializer.Serialize(jsonServer));
    }
}
