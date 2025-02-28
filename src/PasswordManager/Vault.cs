using System.Security.Cryptography;
using System.Text.Json;
using PasswordManager.Keys;

namespace PasswordManager;

public class Vault
{
    public Dictionary<string, string> Values { get; set; } = [];

    public void Set(string prop, string password) => Values[prop] = password;

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

        return Convert.ToBase64String(encryptedVault);
    }

    public static Vault Decrypt(byte[] encryptedVault, VaultKey Key, byte[] IV)
    {
        string vaultJson;
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key.Bytes;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new(encryptedVault);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);

            vaultJson = srDecrypt.ReadToEnd();
        }

        return new()
        {
            Values = JsonSerializer.Deserialize<Dictionary<string, string>>(vaultJson)!
        };
    }
}
