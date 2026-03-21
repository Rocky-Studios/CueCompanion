using CueCompanion.Components;
using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class UserManagementService : StateSubscriberService
{
    private HubConnection? _userManagementHub;
    private UserProvider? _userProvider;

    public async Task StartAsync(string baseUrl, UserProvider userProvider)
    {
        _userProvider = userProvider;

        _userManagementHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}api/user-management")
            .WithAutomaticReconnect()
            .Build();

        await _userManagementHub.StartAsync();
    }

    public async Task<Result<UserInfo[]>> GetUsers(string sessionKey)
    {
        if (_userManagementHub is null)
            throw new InvalidOperationException("UserManagementHub connection is not established.");
        Result<UserInfo[]> result = await _userManagementHub.InvokeAsync<Result<UserInfo[]>>("GetUsers", sessionKey);

        if (result.Error is "Invalid session key.") _userProvider?.RemoveSessionKey();
        if (!result.IsSuccess) return result.Error!;

        return result.Value!;
    }

    public async Task<Result> CreateNewUser(string sessionKey, string userName, string password)
    {
        if (_userManagementHub is null)
            throw new InvalidOperationException("UserManagementHub connection is not established.");
        return await _userManagementHub.InvokeAsync<Result>("CreateNewUser", sessionKey, userName,
            password);
    }

    public async Task DeleteUser(string sessionKey, int userId)
    {
        if (_userManagementHub is null)
            throw new InvalidOperationException("UserManagementHub connection is not established.");
        await _userManagementHub.InvokeAsync("DeleteUser", sessionKey, userId);
    }


    public async Task AddPermissionToUser(string sessionKey, int userID, int permissionID)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        await _userManagementHub.InvokeAsync("AddPermissionToUser", sessionKey, userID, permissionID);
    }

    public async Task RemovePermissionFromUser(string sessionKey, int userID, int permissionID)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        await _userManagementHub.InvokeAsync("RemovePermissionFromUser", sessionKey, userID, permissionID);
    }

    public async Task<Result> EnableLoggingIn(string sessionKey, int userID)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        return await _userManagementHub.InvokeAsync<Result>("EnableLoggingInForUser", sessionKey, userID);
    }

    public async Task<Result> DisableLoggingIn(string sessionKey, int userID)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        return await _userManagementHub.InvokeAsync<Result>("DisableLoggingInForUser", sessionKey, userID);
    }
}