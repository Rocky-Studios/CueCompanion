using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Client;

public class AuthService
{
    private HubConnection? _authHub;
    public Connection? Connection { get; private set; }

    public event Action? OnStateChanged;

    public void SetConnection(Connection connection)
    {
        Connection = connection;
        OnStateChanged?.Invoke();
    }

    public async Task StartAsync(string baseUrl)
    {
        _authHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}authHub")
            .WithAutomaticReconnect()
            .Build();

        await _authHub.StartAsync();
    }

    public async Task<Connection?> ConnectAsync(string connectionName, string password)
    {
        if (_authHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");


        Connection? connection = await _authHub.InvokeAsync<Connection?>("ConnectAsync", connectionName, password);
        if (connection != null) SetConnection(connection);
        else
            Console.WriteLine("Connection failed");

        return connection;
    }
}