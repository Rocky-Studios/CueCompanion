using CueCompanion.Client;

namespace CueCompanion;

public class ConnectionManager
{
    private readonly List<(string connectionName, Guid secret)> ConnectedConnections = [];

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

    public (bool status, string? message) IsConnectionValid(Connection connection)
    {
        bool isConnectionNameValid = Connections.Any(c =>
            c.ConnectionName == connection.ConnectionName);
        if (!isConnectionNameValid)
            return (false, "Connection not found.");
        bool isPasskeyValid = Connections.Any(c =>
            c.ConnectionName == connection.ConnectionName &&
            c.ConnectionPasskey == connection.ConnectionPasskey);
        if (!isPasskeyValid)
            return (false, "Invalid connection passkey.");
        bool isSecretValid = Connections.Any(c =>
            c.ConnectionName == connection.ConnectionName &&
            c.ConnectionPasskey == connection.ConnectionPasskey &&
            c.Secret == connection.Secret);
        if (!isSecretValid)
            return (false, "Invalid connection. Try reconnecting.");
        return (true, null);
    }

    public (Connection? connection, string? error) TryConnect(string connectionName, string connectionKey)
    {
        Connection? connection = Connections.FirstOrDefault(c =>
            c.ConnectionName == connectionName);
        if (connection == null) return (null, "Connection not found.");
        if (connection.ConnectionPasskey != connectionKey)
            return (null, "Invalid connection key.");
        if (ConnectedConnections.Any(c => c.connectionName == connectionName))
            return (null, "Connection already in use.");
        connection = connection.AddSecret();
        ConnectedConnections.Add((connectionName, connection.Secret!.Value));
        return (connection, null);
    }
}