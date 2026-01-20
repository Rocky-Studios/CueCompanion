using CueCompanion.Client;

namespace CueCompanion;

public class ConnectionManager
{
    private readonly List<Connection> Connections =
    [
        new("Master Connection")
    ];

    public void AddConnection(Connection connection)
    {
        Connections.Add(connection);
    }

    public Connection[] GetConnections()
    {
        return Connections.ToArray();
    }

    public (Connection? connection, Exception? error) TryConnect(string connectionName, string connectionKey)
    {
        Connection? connection = Connections.FirstOrDefault(c =>
            c.ConnectionName == connectionName);
        if (connection == null) return (null, new InvalidOperationException("Connection not found."));
        if (connection.ConnectionPasskey != connectionKey)
            return (null, new UnauthorizedAccessException("Invalid connection key."));
        return (connection, null);
    }
}