using System.Security.Cryptography;

namespace CueCompanion.Client;

public class Connection
{
    public Connection(string? connectionName = null)
    {
        ConnectionName = connectionName ?? "New Connection";
    }

    public Connection()
    {
    }

    public string ConnectionName { get; set; } = string.Empty;
    public string ConnectionPasskey { get; set; } = string.Empty;
    public Guid? Secret { get; set; }

    public Connection WithRandomPasskey()
    {
        int value = RandomNumberGenerator.GetInt32(0, 100_000_000);
        ConnectionPasskey = value.ToString("D8");
        return this;
    }

    public Connection AddSecret(Guid? secret = null)
    {
        Secret = secret ?? Guid.NewGuid();
        return this;
    }
}

public enum ClientType
{
    Unknown,
    Master,
    Child
}

public enum UserPermission
{
    View,
    Explore,
    ChangeCueNumber,
    ModifyNotes,
    ModifyShow
}