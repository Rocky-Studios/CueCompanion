using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR;
using UserManager = CueCompanion.UserManagement.UserManager;

namespace CueCompanion.Hubs;

public class UserManagementHub : Hub
{
    public Task<Result<UserInfo[]>> GetUsers(string sessionKey)
    {
        try
        {
            return Task.FromResult(UserManager.GetUsers(sessionKey));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<UserInfo[]>>(exception);
        }
    }

    public Task<Result> CreateNewUser(string sessionKey, string userName, string password)
    {
        try
        {
            return Task.FromResult(UserManager.CreateNewUser(sessionKey, userName, password));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> DeleteUser(string sessionKey, int userId)
    {
        try
        {
            return Task.FromResult(UserManager.DeleteUser(sessionKey, userId));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> RenameUser(string sessionKey, int userID, string newUserName)
    {
        try
        {
            return Task.FromResult(UserManager.RenameUser(sessionKey, userID, newUserName));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> AddPermissionToUser(string sessionKey, int userID, int permissionID)
    {
        try
        {
            return Task.FromResult(UserManager.AddPermissionToUser(sessionKey, userID, permissionID));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> RemovePermissionFromUser(string sessionKey, int userID, int permissionID)
    {
        try
        {
            return Task.FromResult(UserManager.RemovePermissionFromUser(sessionKey, userID, permissionID));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> EnableLoggingInForUser(string sessionKey, int userID)
    {
        try
        {
            return Task.FromResult(UserManager.EnableLoggingInForUser(sessionKey, userID));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> DisableLoggingInForUser(string sessionKey, int userID)
    {
        try
        {
            return Task.FromResult(UserManager.DisableLoggingInForUser(sessionKey, userID));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public async Task<Result> ChangePassword(string sessionKey, string currentPassword, string newPassword)
    {
        return await UserManager.ChangePassword(sessionKey, currentPassword, newPassword);
    }
}