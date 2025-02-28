using System.Text.Json;
using PasswordManager.JsonClasses;
using PasswordManager.Keys;

namespace PasswordManager;

public class Client()
{
    public SecretKey SecretKey { get; set; } = new();

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

    public void WriteToFile(string path)
    {
        JsonClient jsonClient = new()
        {
            SecretKey = SecretKey.String
        };
        File.WriteAllText(path, JsonSerializer.Serialize(jsonClient));
    }
}
