using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class AuthService : StateSubscriberService
{
    public          User?                   User      { get; private set; }
    public          string?                 ApiKey    { get; set; }
    public          bool                    IsLoading { get; set; } = false;
    public readonly Dictionary<string, int> PermissionsCache = new();
    private         HubConnection?          _authHub;


    public void SetUser(User user)
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

    public Task ClearConnectionAsync()
    {
        try
        {
            User = null;
            UpdateState();
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    public async Task<Result<(User user, string apiKey)>> ConnectAsync(string username, string password)
    {
        if (_authHub == null)
            return "Connection is not established. Please wait.";

        Result<(User user, string apiKey)> result =
            await _authHub.InvokeAsync<Result<(User, string)>>("ConnectAsync", username, password);
        if (result.Value.user is { } user) SetUser(user);

        return result;
    }


    public async Task<Result<(User user, string apiKey)>> ConnectAsync(string apiKey)
    {
        if (_authHub == null)
            return "Connection is not established. Please wait.";

        Result<(User user, string apiKey)> result =
            await _authHub.InvokeAsync<Result<(User, string)>>("ConnectAsyncWithKey", apiKey);
        if (result.Value.user is { } user) SetUser(user);

        return result;
    }

    public async Task<Result<Permission[]>> GetPermissionsForUserAsync()
    {
        if (_authHub == null)
            return "AuthHub connection is not established.";

        if (User == null)
            return "User is not connected.";

        return await _authHub.InvokeAsync<Result<Permission[]>>("GetPermissionsForUser", User.Id);
    }

    public async Task<Result<Permission[]>> GetPermissionsAsync()
    {
        if (_authHub == null)
            return "AuthHub connection is not established.";

        Result<Permission[]> result = await _authHub.InvokeAsync<Result<Permission[]>>("GetPermissions");

        if (result is { IsSuccess: true, Value: { } perms })
            foreach (Permission permission in perms)
                PermissionsCache[permission.Name] = permission.Id;

        return result;
    }

    public async Task<Result<User>> GetUserAsync(int userID)
    {
        if (_authHub == null)
            return "AuthHub connection is not established.";

        return await _authHub.InvokeAsync<Result<User>>("GetUser", ApiKey, userID);
    }

    public bool HasPermission(string permissionName, bool valueIfUnknown = false)
    {
        if (User == null) return valueIfUnknown;
        if (!PermissionsCache.TryGetValue(permissionName, out int permissionId)) return valueIfUnknown;

        return User.HasPermission(permissionId);
    }
}