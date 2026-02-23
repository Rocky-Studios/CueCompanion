using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class UserManagementHub : Hub
{
    public async Task<UserInfo[]> GetUsers(string sessionKey)
    {
        return UserManager.GetUsers(sessionKey);
    }
}