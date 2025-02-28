using System;
using System.Security.Cryptography;
using System.Text.Json;

namespace PasswordManager;

public class Vault
{
    public Dictionary<string, string> Values { get; } = [];

    public void Set(string key, string value) => Values[key] = value;

    public byte[] Encrypt(byte[] Key, byte[] IV)
    {
        byte[] encryptedVault;

        string vaultJson = JsonSerializer.Serialize(Values);

        Console.WriteLine(vaultJson);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
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

        return encryptedVault;
    }

    public static Vault Decrypt(byte[] encryptedVault, byte[] Key, byte[] IV)
    {
        string vaultJson;
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new(encryptedVault);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);

            vaultJson = srDecrypt.ReadToEnd();
        }

        Console.WriteLine(vaultJson);

        return JsonSerializer.Deserialize<Vault>(vaultJson)!;
    }
}
