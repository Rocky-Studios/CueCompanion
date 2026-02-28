using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class AuthService : StateSubscriberService
{
    public readonly Dictionary<string, int> PermissionsCache = new();
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
            .WithUrl($"{baseUrl}api/auth")
            .WithAutomaticReconnect()
            .Build();

        await _authHub.StartAsync();

        await GetPermissionsAsync();
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

    public async Task<Permission[]> GetPermissionsForUserAsync()
    {
        if (_authHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        if (User == null)
            throw new InvalidOperationException("User is not connected.");

        return await _authHub.InvokeAsync<Permission[]>("GetPermissionsForUser", User.Id);
    }

    public async Task<Permission[]> GetPermissionsAsync()
    {
        if (_authHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        Permission[] perms = await _authHub.InvokeAsync<Permission[]>("GetPermissions");

        foreach (Permission permission in perms)
        {
            PermissionsCache[permission.Name] = permission.Id;
        }

        return perms;
    }
}