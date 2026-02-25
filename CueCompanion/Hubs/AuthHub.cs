using Microsoft.AspNetCore.SignalR;

namespace CueCompanion;

public class AuthHub : Hub
{
    public async Task<UserConnectionResult> ConnectAsync(string connectionName, string password)
    {
        UserConnectionResult userConnection = DatabaseHandler.TryConnect(connectionName, password);
        return userConnection;
    }

    public async Task<UserConnectionResult> ConnectAsyncWithKey(string connectionKey)
    {
        UserConnectionResult userConnection = DatabaseHandler.TryConnect(connectionKey);
        return userConnection;
    }

    public async Task<Permission[]> GetPermissionsForUser(int userId)
    {
        User user = DatabaseHandler.GetUserById(userId);
        return PermissionManager.GetPermissionsForUser(user).ToArray();
    }
    
    public async Task<Permission[]> GetPermissions()
    {
        return PermissionManager.GetPermissions().ToArray();
    }
}