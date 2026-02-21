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

    public async Task<ConnectionResult> ConnectAsync(string connectionName, string password)
    {
        if (_authHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");


        ConnectionResult connection =
            await _authHub.InvokeAsync<ConnectionResult>("ConnectAsync", connectionName, password);
        if (connection.Connection != null) SetConnection(connection.Connection);

        return connection;
    }
}