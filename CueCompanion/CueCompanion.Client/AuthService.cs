using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Client;

public class AuthService
{
    private HubConnection? _connection;
    public User? User { get; private set; }
    public string? ConnectionKey { get; }
    public event Func<Task>? OnChange;

    public async Task StartAsync(string baseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}authHub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<User>("UserConnected", user =>
        {
            User = user;
            OnChange?.Invoke();
        });

        await _connection.StartAsync();
        OnChange?.Invoke();
    }

    public async Task<Connection[]> GetConnectionsAsync()
    {
        if (_connection == null) throw new InvalidOperationException("Connection not started.");

        ConnectionsPacket packet = await _connection.InvokeAsync<ConnectionsPacket>("GetConnections", User);
        if (packet.Error != null) Console.WriteLine("Failed to get connections: " + packet.Error);

        return packet.Connections ?? [];
    }

    public async Task WaitForUser()
    {
        while (User == null) await Task.Delay(10);
    }

    public async Task<Connection> ConnectAsync(User user, string connectionName, string connectionKey)
    {
        (Connection? connection, Exception? error) =
            await _connection.InvokeAsync<(Connection? connection, Exception? error)>("Connect", user, connectionName,
                connectionKey);
        if (error != null)
        {
            Console.WriteLine("Failed to connect to connection: " + connectionName + " Error: " + error.Message);
            throw error;
        }

        Console.WriteLine("Connected to connection: " + connectionName);
        return connection;
    }
}