using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class AuthService : StateSubscriberService
{
    private HubConnection? _authHub;
    public User? User { get; private set; }
    public string? SessionKey { get; set; }
    public bool isLoading { get; set; } = false;



    public void SetConnection(User user)
    {
        User = user;
        UpdateState();
    }

    public async Task StartAsync(string baseUrl)
    {
        _authHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}auth")
            .WithAutomaticReconnect()
            .Build();

        await _authHub.StartAsync();
    }

    public async Task ClearConnectionAsync()
    {
        User = null;
        UpdateState();
    }

    public async Task<UserConnectionResult> ConnectAsync(string connectionName, string password)
    {
        if (_authHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");


        UserConnectionResult userConnection =
            await _authHub.InvokeAsync<UserConnectionResult>("ConnectAsync", connectionName, password);
        if (userConnection.User != null) SetConnection(userConnection.User);

        return userConnection;
    }


    public async Task<UserConnectionResult> ConnectAsync(string connectionKey)
    {
        if (_authHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");


        UserConnectionResult userConnection =
            await _authHub.InvokeAsync<UserConnectionResult>("ConnectAsyncWithKey", connectionKey);
        if (userConnection.User != null) SetConnection(userConnection.User);

        return userConnection;
    }
    
    public async Task<Permission[]> GetPermissionsAsync()
    {
        if (_authHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        if (User == null)
            throw new InvalidOperationException("User is not connected.");

        return await _authHub.InvokeAsync<Permission[]>("GetPermissions", User.Id);
    }
}