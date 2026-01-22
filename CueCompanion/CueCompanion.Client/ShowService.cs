using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion;

public class ShowService
{
    private readonly AuthService _auth;
    private HubConnection? _connection;

    public ShowService(AuthService auth)
    {
        _auth = auth;
    }

    public ServerState? State { get; private set; } = new();

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
        await GetShowState();

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

    public async Task UpdateEntireState()
    {
        if (_connection != null)
            await _connection.InvokeAsync("UpdateEntireState", State);
    }

    public async Task GetShowState()
    {
        if (_connection != null && _auth.Connection != null)
            State = await _connection.InvokeAsync<ServerState?>("GetState", _auth.Connection);
    }
}