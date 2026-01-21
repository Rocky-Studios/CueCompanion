using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Client;

public class AuthService
{
    private HubConnection? _hubConnection;

    public ClientType ClientType { get; set; } = ClientType.Unknown;
    public event Func<Task>? OnChange;

    public async Task StartAsync(string baseUrl)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}authHub")
            .WithAutomaticReconnect()
            .Build();


        _hubConnection.On("ClientTypeVerification", (ClientType clientType) =>
        {
            ClientType = clientType;
            OnChange?.Invoke();
        });

        await _hubConnection.StartAsync();
        OnChange?.Invoke();
    }

    //public async Task WaitForUser()
    //{
    //    while (User == null) await Task.Delay(10);
    //}
}