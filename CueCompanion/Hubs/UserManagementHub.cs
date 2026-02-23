using CueCompanion.Components;
using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR;
using UserManager = CueCompanion.UserManagement.UserManager;

namespace CueCompanion.Hubs;

public class UserManagementHub : Hub
{
    public async Task<UserInfo[]> GetUsers(string sessionKey)
    {
        return UserManager.GetUsers(sessionKey);
    }
    
    public async Task<CreateNewUserResult> CreateNewUser(string sessionKey, string userName, string password)
    {
        return UserManager.CreateNewUser(sessionKey, userName, password);
    }
}