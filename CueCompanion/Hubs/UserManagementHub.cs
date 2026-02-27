using CueCompanion.Components;
using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR;
using UserManager = CueCompanion.UserManagement.UserManager;

namespace CueCompanion.Hubs;

public class UserManagementHub : Hub
{
    public async Task<Or<UserInfo[], string>> GetUsers(string sessionKey)
    {
        return UserManager.GetUsers(sessionKey);
    }
    
    public async Task<CreateNewUserResult> CreateNewUser(string sessionKey, string userName, string password)
    {
        return UserManager.CreateNewUser(sessionKey, userName, password);
    }
    
    public async Task DeleteUser(string sessionKey, int userId)
    {
        UserManager.DeleteUser(sessionKey, userId);
    }

    public async Task AddPermissionToUser(string sessionKey, int userID, int permissionID)
    {
        UserManager.AddPermissionToUser(sessionKey, userID, permissionID);
    }
    
    public async Task RemovePermissionFromUser(string sessionKey, int userID, int permissionID)
    {
        UserManager.RemovePermissionFromUser(sessionKey, userID, permissionID);
    }
}