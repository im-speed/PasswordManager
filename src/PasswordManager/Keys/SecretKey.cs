using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Keys;

public class SecretKey
{
    public byte[] Bytes { get; }

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

    public override string ToString() => Encoding.Unicode.GetString(Bytes);
}
