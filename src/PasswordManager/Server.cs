using System.Security.Cryptography;
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
    public static Server ReadFromFile(string path, VaultKey vaultKey)
    {
        JsonServer? jsonServer = JsonSerializer.Deserialize<JsonServer>(
            File.ReadAllText(path)
        )!;

        byte[] IV = Convert.FromBase64String(jsonServer.IV);
        byte[] encryptedVault = Convert.FromBase64String(jsonServer.Vault);

        return new()
        {
            IV = IV,
            Vault = Vault.Decrypt(encryptedVault, vaultKey, IV)
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
