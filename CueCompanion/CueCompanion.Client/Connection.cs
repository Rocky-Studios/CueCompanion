namespace CueCompanion.Client;

public class Connection
{
    public Connection()
    {
    }

    public Connection(string connectionName)
    {
        ConnectionName = connectionName;
    }

    public string ConnectionName { get; set; } = "";
    public Dictionary<string, bool> Viewing { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}