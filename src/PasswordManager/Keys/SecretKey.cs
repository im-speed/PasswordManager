using System.Security.Cryptography;

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

    public SecretKey(string secretKey)
    {
        Bytes = Convert.FromBase64String(secretKey);
    }

    /// <summary>
    /// Tries to create a new secret key class from a secret key string.
    /// </summary>
    /// <returns>A new secret key class or null if the string could not be parsed.</returns>
    public static SecretKey? FromString(string secretKey)
    {
        try
        {
            return new SecretKey(secretKey);
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid secret key.");
            return null;
        }
    }

    public override string ToString() => Convert.ToBase64String(Bytes);
}
