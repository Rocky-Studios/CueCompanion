
using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion;

public class CounterService
{
    public ServerState State { get; private set; } = new();
    public Connection UserConnection { get; set; }
    private HubConnection? _connection;

    public event Func<Task>? OnChange;

    public async Task StartAsync(string baseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}cueHub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<ServerState>("StateUpdated", async state =>
        {
            State = state;
            if (OnChange != null)
                await OnChange.Invoke();
        });

        await _connection.StartAsync();

        State = await _connection.InvokeAsync<ServerState>("GetState");
        UserConnection = State.Connections[0];
        UserConnection.Viewing =
        [
            ("sound", true)
        ];
        if (OnChange != null)
            await OnChange.Invoke();
    }

    public async Task UpdateCueNumber(int newCueNumber)
    {
        if (_connection != null)
            await _connection.InvokeAsync("UpdateCueNumber", newCueNumber);
    }
}

