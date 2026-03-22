using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion;

public class AuthHub : Hub
{
    public Result<(User user, string sesionKey)> ConnectAsync(string connectionName, string password)
    {
        return UserManager.TryConnect(connectionName, password);
    }

    public Result<(User user, string sesionKey)> ConnectAsyncWithKey(string connectionKey)
    {
        return UserManager.TryConnect(connectionKey);
    }

    public Result<Permission[]> GetPermissionsForUser(int userId)
    {
        User? user = UserManager.GetUserById(userId);
        if (user == null) return "User not found.";
        return PermissionManager.GetPermissionsForUser(user);
    }

    public Result<Permission[]> GetPermissions()
    {
        return PermissionManager.GetPermissions().ToArray();
    }

    public Result<User> GetUser(string sessionKey, int userId)
    {
        User? user = UserManager.GetUserById(userId);
        if (user == null) return "User not found.";
        user.PasswordHash = "";
        user.Password = ""; // Don't send the password to the client
        return user;
    }
}