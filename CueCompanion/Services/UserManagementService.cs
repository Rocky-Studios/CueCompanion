using System.Diagnostics.CodeAnalysis;
using CueCompanion.Components;
using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class UserManagementService : StateSubscriberService, IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        if (_userManagementHub != null)
            await _userManagementHub.DisposeAsync();
    }

    private HubConnection? _userManagementHub;
    private UserProvider?  _userProvider;

    public async Task StartAsync(UserProvider userProvider)
    {
        _userProvider = userProvider;

        _userManagementHub = new HubConnectionBuilder()
                            .WithUrl($"{Program.localhostURL}/api/user-management")
                            .WithAutomaticReconnect()
                            .Build();

        await _userManagementHub.StartAsync();
    }

    public async Task<Result<UserInfo[]>> GetUsers(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;
        var result = await hub.InvokeAsync<Result<UserInfo[]>>("GetUsers", sessionKey);

        if (result.Error is "Invalid session key.") _userProvider?.RemoveSessionKey();
        if (!result.IsSuccess) return result.Error!;

        return result.Value!;
    }

    public async Task<Result> CreateNewUser(string sessionKey, string userName, string password)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;
        return await hub.InvokeAsync<Result>("CreateNewUser", sessionKey, userName, password);
    }

    public async Task<Result> RenameUser(string sessionKey, int userID, string newUserName)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;
        return await hub.InvokeAsync<Result>("RenameUser", sessionKey, userID, newUserName);
    }

    public async Task DeleteUser(string sessionKey, int userId)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out _)) return;
        await hub.InvokeAsync("DeleteUser", sessionKey, userId);
    }

    public async Task AddPermissionToUser(string sessionKey, int userID, int permissionID)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out _)) return;

        await hub.InvokeAsync("AddPermissionToUser", sessionKey, userID, permissionID);
    }

    public async Task RemovePermissionFromUser(string sessionKey, int userID, int permissionID)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out _)) return;

        await hub.InvokeAsync("RemovePermissionFromUser", sessionKey, userID, permissionID);
    }

    public async Task<Result> EnableLoggingIn(string sessionKey, int userID)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("EnableLoggingInForUser", sessionKey, userID);
    }

    public async Task<Result> DisableLoggingIn(string sessionKey, int userID)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("DisableLoggingInForUser", sessionKey, userID);
    }

    public async Task<Result> ChangePassword(string sessionKey, string currentPassword, string newPassword)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("ChangePassword", sessionKey, currentPassword, newPassword);
    }

    private bool TryGetConnectedHub([NotNullWhen(true)] out HubConnection? hub, out string? error)
    {
        hub = _userManagementHub;
        if (hub is null)
        {
            error = "UserManagementHub connection is not established.";
            return false;
        }

        if (hub.State != HubConnectionState.Connected)
        {
            error = "UserManagementHub connection is not active.";
            return false;
        }

        error = null;
        return true;
    }
}