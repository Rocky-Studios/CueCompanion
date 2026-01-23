using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Client;

public class AuthService
{
    private HubConnection? _authHub;

    public ClientType? ClientType { get; set; }
    public Connection? Connection { get; set; }
    public string? ConnectionMessage { get; set; }
    public event Func<Task>? OnChange;

    public async Task StartAsync(string baseUrl)
    {
        _authHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}authHub")
            .WithAutomaticReconnect()
            .Build();


        _authHub.On("ClientTypeVerification", (ClientType clientType) =>
        {
            ClientType = clientType;
            OnChange?.Invoke();
        });

        await _authHub.StartAsync();
        OnChange?.Invoke();
    }

    private async Task WaitForConnection()
    {
        while (_authHub == null) await Task.Delay(10);
    }

    //public async Task WaitForUser()
    //{
    //    while (User == null) await Task.Delay(10);
    //}

    public async Task<Connection?> Connect(string connectionName, string connectionPasskey)
    {
        await WaitForConnection();
        UserConnectPacket packet =
            await _authHub.InvokeAsync<UserConnectPacket>("Connect", connectionName, connectionPasskey);
        Connection = packet.Connection;
        ConnectionMessage = null;
        if (packet.Error != null)
            ConnectionMessage = packet.Error;
        else if (packet.Message != null)
            ConnectionMessage += packet.Message;
        else ConnectionMessage = "Connected successfully.";

        OnChange?.Invoke();
        return Connection;
    }
}