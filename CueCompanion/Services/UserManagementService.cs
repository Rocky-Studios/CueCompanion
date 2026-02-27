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
            .WithUrl($"{baseUrl}user-management")
            .WithAutomaticReconnect()
            .Build();

        await _userManagementHub.StartAsync();
    }

    public async Task<UserInfo[]> GetUsers(string sessionKey)
    {
        if(_userManagementHub is null) throw new InvalidOperationException("UserManagementHub connection is not established.");
        var result = await _userManagementHub.InvokeAsync<Or<UserInfo[],string>>("GetUsers", sessionKey);
        if(result.Option1 is {} users) return users;
        if (result.Option2 is "Invalid session key.") await _userProvider.RemoveSessionKey();
        Console.WriteLine("Error getting users:" + result.Option2);
        return [];
    }
    
    public async Task<CreateNewUserResult> CreateNewUser(string sessionKey, string userName, string password)
    {
        if(_userManagementHub is null) throw new InvalidOperationException("UserManagementHub connection is not established.");
        return await _userManagementHub.InvokeAsync<CreateNewUserResult>("CreateNewUser", sessionKey, userName, password);
    }
    
    public async Task DeleteUser(string sessionKey, int userId)
    {
        if(_userManagementHub is null) throw new InvalidOperationException("UserManagementHub connection is not established.");
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
}