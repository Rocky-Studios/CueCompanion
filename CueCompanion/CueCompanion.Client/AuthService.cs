using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Client;

public class AuthService
{
    private HubConnection? _connection;
    public User? User { get; private set; }
    public event Action? OnUserChanged;

    public async Task StartAsync(string baseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}authHub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<User>("Authenticated", user =>
        {
            User = user;
            OnUserChanged?.Invoke();
        });

        _connection.Reconnecting += error =>
        {
            // DO NOT clear User here
            return Task.CompletedTask;
        };


        await _connection.StartAsync();
    }
}