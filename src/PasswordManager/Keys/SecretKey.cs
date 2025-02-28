using System.Security.Cryptography;

namespace PasswordManager.Keys;

public class SecretKey
{
    public byte[] Bytes { get; }
    public string String => Convert.ToBase64String(Bytes);

    public SecretKey()
    {
        byte[] key = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        Bytes = key;
    }

    public SecretKey(byte[] secretKey)
    {
        Bytes = secretKey;
    }

    public override string ToString() => Convert.ToBase64String(Bytes);
}
