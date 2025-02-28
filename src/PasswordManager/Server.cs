using System.Security.Cryptography;
using System.Text.Json;
using PasswordManager.JsonClasses;
using PasswordManager.Keys;

namespace PasswordManager;

public class Server()
{
    public byte[] IV { get; set; } = Aes.Create().IV;

    public Vault Vault { get; set; } = new();

    public void WriteToFile(string path, VaultKey vaultKey)
    {
        JsonServer jsonServer = new()
        {
            IV = Convert.ToBase64String(IV),
            Vault = Vault.Encrypt(vaultKey, IV)
        };

        File.WriteAllText(path, JsonSerializer.Serialize(jsonServer));
    }

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
}
