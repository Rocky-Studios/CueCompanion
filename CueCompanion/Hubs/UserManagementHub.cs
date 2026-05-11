using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR;
using UserManager = CueCompanion.UserManagement.UserManager;

namespace CueCompanion.Hubs;

public class UserManagementHub : Hub
{
    public Task<Result<UserInfo[]>> GetUsers(string apiKey)
    {
        try
        {
            return Task.FromResult(UserManager.GetUsers(apiKey));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<UserInfo[]>>(exception);
        }
    }

    public Task<Result> CreateNewUser(string apiKey, string userName, string password)
    {
        try
        {
            return Task.FromResult(UserManager.CreateNewUser(apiKey, userName, password));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> DeleteUser(string apiKey, int userId)
    {
        try
        {
            return Task.FromResult(UserManager.DeleteUser(apiKey, userId));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> RenameUser(string apiKey, int userID, string newUserName)
    {
        try
        {
            return Task.FromResult(UserManager.RenameUser(apiKey, userID, newUserName));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> AddPermissionToUser(string apiKey, int userID, int permissionID)
    {
        try
        {
            return Task.FromResult(UserManager.AddPermissionToUser(apiKey, userID, permissionID));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> RemovePermissionFromUser(string apiKey, int userID, int permissionID)
    {
        try
        {
            return Task.FromResult(UserManager.RemovePermissionFromUser(apiKey, userID, permissionID));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> EnableLoggingInForUser(string apiKey, int userID)
    {
        try
        {
            return Task.FromResult(UserManager.SetLoggingInForUser(apiKey, userID, true));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> DisableLoggingInForUser(string apiKey, int userID)
    {
        try
        {
            return Task.FromResult(UserManager.SetLoggingInForUser(apiKey, userID, false));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public async Task<Result> ChangePassword(string apiKey, string currentPassword, string newPassword) =>
        UserManager.ChangePassword(apiKey, currentPassword, newPassword);
}