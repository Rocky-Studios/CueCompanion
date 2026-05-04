using CueCompanion.Components;
using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class UserManagementService : StateSubscriberService
{
    private HubConnection? _userManagementHub;
    private UserProvider?  _userProvider;
    public  UserInfo[]     UserInfos = [];

    public async Task StartAsync(string baseUrl, UserProvider userProvider)
    {
        _userProvider = userProvider;

        _userManagementHub = new HubConnectionBuilder()
                            .WithUrl($"{baseUrl}api/user-management")
                            .WithAutomaticReconnect()
                            .Build();

        await _userManagementHub.StartAsync();
    }

    public async Task<Result<UserInfo[]>> GetUsers(string apiKey)
    {
        if (_userManagementHub is null)
            throw new InvalidOperationException("UserManagementHub connection is not established.");
        Result<UserInfo[]> result = await _userManagementHub.InvokeAsync<Result<UserInfo[]>>("GetUsers", apiKey);

        if (result.Error is "Invalid api key.") _userProvider?.RemoveApiKey();
        if (!result.IsSuccess) return result.Error!;

        return result.Value!;
    }

    public async Task<Result> CreateNewUser(string apiKey, string userName, string password)
    {
        if (_userManagementHub is null)
            throw new InvalidOperationException("UserManagementHub connection is not established.");
        return await _userManagementHub.InvokeAsync<Result>("CreateNewUser", apiKey, userName,
                                                            password);
    }

    public async Task<Result> RenameUser(string apiKey, int userID, string newUserName)
    {
        if (_userManagementHub is null)
            throw new InvalidOperationException("UserManagementHub connection is not established.");
        return await _userManagementHub.InvokeAsync<Result>("RenameUser", apiKey, userID, newUserName);
    }

    public async Task DeleteUser(string apiKey, int userId)
    {
        if (_userManagementHub is null)
            throw new InvalidOperationException("UserManagementHub connection is not established.");
        await _userManagementHub.InvokeAsync("DeleteUser", apiKey, userId);
    }

    public async Task AddPermissionToUser(string apiKey, int userID, int permissionID)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        await _userManagementHub.InvokeAsync("AddPermissionToUser", apiKey, userID, permissionID);
    }

    public async Task RemovePermissionFromUser(string apiKey, int userID, int permissionID)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        await _userManagementHub.InvokeAsync("RemovePermissionFromUser", apiKey, userID, permissionID);
    }

    public async Task<Result> EnableLoggingIn(string apiKey, int userID)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        return await _userManagementHub.InvokeAsync<Result>("EnableLoggingInForUser", apiKey, userID);
    }

    public async Task<Result> DisableLoggingIn(string apiKey, int userID)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        return await _userManagementHub.InvokeAsync<Result>("DisableLoggingInForUser", apiKey, userID);
    }

    public async Task<Result> ChangePassword(string apiKey, string currentPassword, string newPassword)
    {
        if (_userManagementHub == null)
            throw new InvalidOperationException("AuthHub connection is not established.");

        return await _userManagementHub.InvokeAsync<Result>("ChangePassword", apiKey, currentPassword, newPassword);
    }
}