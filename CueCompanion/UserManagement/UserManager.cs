using SQLite;

namespace CueCompanion.UserManagement;

public static class UserManager
{
    private static SQLiteConnection _db => DatabaseHandler.Connection;
    
    public static UserInfo[] GetUsers(string sessionKey)
    {
        User? user =GetUserBySessionKey(sessionKey);
        if (user == null) throw new Exception("Invalid session key.");
        bool hasManageUsersPermission = PermissionManager.HasPermission(
            PermissionManager.GetPermissionByName("ManageUsers") ?? throw new Exception("ManageUsers permission not found."),
            user);
        if (!hasManageUsersPermission) throw new Exception("Access denied.");
        
        User[] users = _db.Table<User>().ToArray();
        UserInfo[] userInfos = users.Select(u => new UserInfo()
        {
            UserID = u.Id,
            UserName = u.UserName,
            Permissions = GetPerms(u.Id)
        }).ToArray();


        Permission[] GetPerms(int userId)
        {
            User u = users.First(u => u.Id == userId);
            return PermissionManager.GetPermissionsForUser(u).ToArray();
        }
        
        return userInfos;
    }
    
    public static User? GetUserBySessionKey(string sessionKey)
    {
        SessionKey? session = _db.Table<SessionKey>().FirstOrDefault(sk => sk.Key == sessionKey, null);
        if (session == null) throw new Exception("Invalid session key.");
        User? user = _db.Table<User>().FirstOrDefault(u => u.Id == session.ConnectionId, null);
        if (user == null) return
        null;
        return user;
    }
}