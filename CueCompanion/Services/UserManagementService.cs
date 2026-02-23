using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class UserManagementService
{
    
    private HubConnection? _userManagementHub;
    public async Task StartAsync(string baseUrl)
    {
        _userManagementHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}user-management")
            .WithAutomaticReconnect()
            .Build();

        await _userManagementHub.StartAsync();
    }

    public async Task<UserInfo[]> GetUsers(string sessionKey)
    {
        if(_userManagementHub is null) throw new InvalidOperationException("UserManagementHub connection is not established.");
        UserInfo[] users = await _userManagementHub.InvokeAsync<UserInfo[]>("GetUsers", sessionKey);
        return users;
    }
}