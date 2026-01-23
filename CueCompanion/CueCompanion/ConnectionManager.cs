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

    public void ResetConnectedConnections()
    {
        ConnectedConnections.Clear();
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

    public (Connection? connection, string? error) TryConnect(string connectionName, string connectionKey, Guid? secret)
    {
        // Try to find the connection by name
        Connection? connection = Connections.FirstOrDefault(c => c.ConnectionName == connectionName);
        if (connection == null) return (null, "Connection not found.");

        // Key mismatch, obviously invalid
        if (connection.ConnectionPasskey != connectionKey)
            return (null, "Invalid connection key.");

        // Add a random secret to the connection
        connection = connection.AddSecret();
        // If a secret was provided, validate it
        if (secret is { } s)
        {
            // Override the randomly generated secret with the provided one
            connection.Secret = s;

            if (ConnectedConnections.All(c => c.secret != s))
                return (null, "Invalid connection.");
        }
        else
        {
            // No secret provided, so if the connection is already in use, reject it
            if (ConnectedConnections.Any(c => c.connectionName == connectionName))
                return (null, "Connection already in use.");

            // Otherwise, add it to the list of connected connections
            ConnectedConnections.Add((connectionName, connection.Secret!.Value));
        }

        return (connection, null);
    }
}