using System.Security.Cryptography;

namespace PasswordManager.Keys;

public class VaultKey
{
    public byte[] Bytes { get; }

    public VaultKey(string masterPassword, SecretKey secretKey)
    {
        Rfc2898DeriveBytes rfc2898 = new(
            masterPassword, secretKey.Bytes, 10000, HashAlgorithmName.SHA256
        );

        Bytes = rfc2898.GetBytes(16);
    }
}
