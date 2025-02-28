using System.Text.Json;

namespace PasswordManager;

public class Server(string IV)
{
    public string IV { get; set; } = IV;

    public void WriteToFile(string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(this));
    }
}
