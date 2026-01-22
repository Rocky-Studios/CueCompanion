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

    public ShowState? State { get; private set; }
    public Show? Show => State?.CurrentShow;
    public Cue? CurrentCue => State?.CurrentCue;
    public Dictionary<string, string>? CurrentTasks => CurrentCue?.Tasks;
    public int? CurrentCueNumber => State?.CurrentCueNumber;
    public event Func<Task>? OnChange;

    public async Task StartAsync(string baseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}cueHub")
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync();

        if (OnChange != null)
            await OnChange.Invoke();
    }

    public async Task GetShow()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection has not been started.");
        if (_auth.Connection == null)
            throw new InvalidOperationException("No valid connection in AuthService.");
        ShowResponsePacket response = await _connection.InvokeAsync<ShowResponsePacket>("GetShow", _auth.Connection);
        if (response.ErrorMessage is { Length: > 0 })
            Console.WriteLine("Error requesting show: " + response.ErrorMessage);

        State = response.State;
        if (OnChange != null)
            await OnChange.Invoke();
    }
}