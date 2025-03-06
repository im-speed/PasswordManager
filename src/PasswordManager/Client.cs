using System.Text.Json;
using PasswordManager.JsonClasses;
using PasswordManager.Keys;

namespace PasswordManager;

public class Client()
{
    public SecretKey SecretKey { get; set; } = new();

    /// <summary>
    /// Creates a client by reading it from a json file.
    /// </summary>
    internal static Client? ReadFromFile(string path)
    {
        JsonClient jsonClient;
        SecretKey secretKey;
        try
        {
            jsonClient = JsonSerializer.Deserialize<JsonClient>(File.ReadAllText(path))!;
            secretKey = new(jsonClient.SecretKey);
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to read client from file. Try running 'init' or 'create'.");
            return null;
        }

        return new()
        {
            SecretKey = secretKey
        };
    }

    /// <summary>
    /// Writes the client to a json file, overwriting it if it already exists.
    /// </summary>
    public void WriteToFile(string path)
    {
        JsonClient jsonClient = new()
        {
            SecretKey = SecretKey.ToString()
        };
        File.WriteAllText(path, JsonSerializer.Serialize(jsonClient));
    }
}
