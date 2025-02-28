using System.Text.Json;
using System.Text.Json.Serialization;

namespace PasswordManager;

public class Client(string secretKey)
{
    [JsonPropertyName("secretKey")]
    public string SecretKey { get; set; } = secretKey;

    public void WriteToFile(string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(this));
    }
}
