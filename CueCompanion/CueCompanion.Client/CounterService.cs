using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion;

public class CounterService
{
    private HubConnection? _connection;
    public ServerState State { get; private set; } = new();
    public Connection UserConnection { get; set; }

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
        UserConnection.Viewing = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            { "Sound", true }
        };
        if (OnChange != null)
            await OnChange.Invoke();
    }

    public async Task UpdateCueNumber(int newCueNumber)
    {
        if (_connection != null)
            await _connection.InvokeAsync("UpdateCueNumber", newCueNumber);
    }

    public async Task UpdateNote(string noteID, string newNoteText)
    {
        if (_connection != null)
            await _connection.InvokeAsync("UpdateNote", noteID, newNoteText);
    }
}