using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PasswordManager.Keys;

namespace PasswordManager;

/// <summary>
/// A encryptable vault of passwords.
/// </summary>
/// <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-6.0"/>
public class Vault
{
    public Dictionary<string, string> Values { get; set; } = [];

    public void Set(string prop, string password) => Values[prop] = password;

    public string? Get(string prop)
    {
        if (Values.TryGetValue(prop, out string? value))
        {
            return value;
        }
        else
        {
            Console.WriteLine($"The property '{prop}' does not exist.");
            return null;
        }
    }

    /// <summary>
    /// Encrypts the vault into a string.
    /// </summary>
    /// <param name="Key">The key used to encrypt the vault.</param>
    /// <param name="IV">The initialization vector used to encrypt the vault.</param>
    public string Encrypt(VaultKey Key, byte[] IV)
    {
        byte[] encryptedVault;

        string vaultJson = JsonSerializer.Serialize(Values);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key.Bytes;
            aesAlg.IV = IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new();
            using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter swEncrypt = new(csEncrypt);
                swEncrypt.Write(vaultJson);
            }

            encryptedVault = msEncrypt.ToArray();
        }

        return Encoding.Unicode.GetString(encryptedVault);
    }

    /// <summary>
    /// Decrypts the vault from a byte array.
    /// </summary>
    /// <param name="encryptedVault">The byte array vault.</param>
    /// <param name="Key">The key used to encrypt the vault.</param>
    /// <param name="IV">The initialization vector used to encrypt the vault.</param>
    public static Vault? Decrypt(byte[] encryptedVault, VaultKey Key, byte[] IV)
    {
        Dictionary<string, string> decryptedValues;
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key.Bytes;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new(encryptedVault);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);

            try
            {
                decryptedValues = JsonSerializer.Deserialize<Dictionary<string, string>>(srDecrypt.ReadToEnd())!;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to decrypt vault. Master password or secret key may be incorrect.");
                return null;
            }
        }

        return new()
        {
            Values = decryptedValues
        };
    }
}
