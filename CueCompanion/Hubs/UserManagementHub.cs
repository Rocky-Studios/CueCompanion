using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR;
using UserManager = CueCompanion.UserManagement.UserManager;

namespace CueCompanion.Hubs;

public class UserManagementHub : Hub
{
    public async Task<Result<UserInfo[]>> GetUsers(string sessionKey)
    {
        return UserManager.GetUsers(sessionKey);
    }

    public async Task<Result> CreateNewUser(string sessionKey, string userName, string password)
    {
        return UserManager.CreateNewUser(sessionKey, userName, password);
    }

    public async Task<Result> DeleteUser(string sessionKey, int userId)
    {
        return UserManager.DeleteUser(sessionKey, userId);
    }

    public async Task<Result> AddPermissionToUser(string sessionKey, int userID, int permissionID)
    {
        return UserManager.AddPermissionToUser(sessionKey, userID, permissionID);
    }

    public async Task<Result> RemovePermissionFromUser(string sessionKey, int userID, int permissionID)
    {
        return UserManager.RemovePermissionFromUser(sessionKey, userID, permissionID);
    }
}