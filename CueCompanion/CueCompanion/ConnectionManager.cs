using CueCompanion.Client;

namespace CueCompanion;

public class ConnectionManager
{
    private readonly List<Connection> Connections =
    [
        new Connection("Master Connection").WithRandomPasskey()
    ];

    public void AddConnection(Connection connection)
    {
        Connections.Add(connection);
    }

    public Connection[] GetConnections()
    {
        return Connections.ToArray();
    }

    public bool IsConnectionValid(Connection connection)
    {
        return Connections.Any(c =>
            c.ConnectionName == connection.ConnectionName &&
            c.ConnectionPasskey == connection.ConnectionPasskey);
    }

    public (Connection? connection, string? error) TryConnect(string connectionName, string connectionKey)
    {
        Connection? connection = Connections.FirstOrDefault(c =>
            c.ConnectionName == connectionName);
        if (connection == null) return (null, "Connection not found.");
        if (connection.ConnectionPasskey != connectionKey)
            return (null, "Invalid connection key.");
        connection = connection.WithSecret();
        return (connection, null);
    }
}