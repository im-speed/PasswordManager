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
    internal static Client ReadFromFile(string path)
    {
        JsonClient jsonClient = JsonSerializer.Deserialize<JsonClient>(
            File.ReadAllText(path)
        )!;

        return new()
        {
            SecretKey = new(Convert.FromBase64String(jsonClient.SecretKey))
        };
    }

    /// <summary>
    /// Writes the client to a json file, overwriting it if it already exists.
    /// </summary>
    public void WriteToFile(string path)
    {
        JsonClient jsonClient = new()
        {
            SecretKey = SecretKey.String
        };
        File.WriteAllText(path, JsonSerializer.Serialize(jsonClient));
    }
}
