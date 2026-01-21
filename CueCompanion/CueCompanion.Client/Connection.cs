using System.Security.Cryptography;

namespace CueCompanion.Client;

public class Connection
{
    public Connection(string? connectionName = null)
    {
        ConnectionName = connectionName ?? "New Connection";
        int value = RandomNumberGenerator.GetInt32(0, 100_000_000);
        ConnectionPasskey = value.ToString("D8");
    }

    public Connection()
    {
    }

    public string ConnectionName { get; set; } = string.Empty;
    public string ConnectionPasskey { get; set; } = string.Empty;
    public Guid? ConnectedUserId { get; set; }
}