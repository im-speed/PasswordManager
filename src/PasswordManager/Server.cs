using System.Text.Json;
using System.Text.Json.Serialization;

namespace PasswordManager;

public class Server(string IV)
{
    public string IV { get; set; } = IV;

    [JsonPropertyName("vault")]
    public string EncryptedVault { get; set; } = "";

    [JsonIgnore]
    public Vault Vault { get; set; } = new();

    public void WriteToFile(string path, byte[] vaultKey)
    {
        EncryptedVault = Convert.ToBase64String(Vault.Encrypt(vaultKey, Convert.FromBase64String(IV)));
        File.WriteAllText(path, JsonSerializer.Serialize(this));
    }

    public static Server ReadFromFile(string path, byte[] vaultKey)
    {
        Server server = JsonSerializer.Deserialize<Server>(File.ReadAllText(path))!;

        if (server.EncryptedVault == "")
        {
            return server;
        }

        server.Vault = Vault.Decrypt(
            Convert.FromBase64String(server.EncryptedVault),
            vaultKey,
            Convert.FromBase64String(server.IV)
        );

        foreach (KeyValuePair<string, string> pair in server.Vault.Values)
        {
            Console.WriteLine(pair.Key + ": " + pair.Value);
        }

        return server;
    }
}
