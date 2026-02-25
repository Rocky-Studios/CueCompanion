using CueCompanion.Components;
using SQLite;

namespace CueCompanion.UserManagement;

public static class UserManager
{
    private static SQLiteConnection _db => DatabaseHandler.Connection;
    
    private static bool HasManageUsersPermission(string sessionKey)
    {
        User? user =GetUserBySessionKey(sessionKey);
        if (user == null) throw new Exception("Invalid session key.");
        bool hasManageUsersPermission = PermissionManager.HasPermission(
            PermissionManager.GetPermissionByName("ManageUsers") ?? throw new Exception("ManageUsers permission not found."),
            user);
        if (!hasManageUsersPermission) throw new Exception("Access denied.");
        return true;
    }
    
    public static UserInfo[] GetUsers(string sessionKey)
    {
        User? user =GetUserBySessionKey(sessionKey);
        if (user == null) throw new Exception("Invalid session key.");
        if(!HasManageUsersPermission(sessionKey)) throw new Exception("Access denied.");
        
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
    
    public static CreateNewUserResult CreateNewUser(string sessionKey, string userName, string password)
    {
        if(!HasManageUsersPermission(sessionKey)) throw new Exception("Access denied.");
        
        string? errorMessage = null;
        if (_db.Table<User>().Any(u => u.UserName == userName))
        {
            errorMessage = "Username already exists.";
            return new CreateNewUserResult(errorMessage);
        }
        if (!ValidateUsername(userName))
        {
            errorMessage = "Invalid username. Usernames must be 3-20 characters long and can only contain letters, digits, and underscores.";
            return new CreateNewUserResult(errorMessage);
        }
        
        User newUser = new ()
        {
            UserName = userName,
            PasswordHash = Hash.HashPassword(password),
        };
        _db.Insert(newUser);
        return new CreateNewUserResult();
    }
    
    private static bool ValidateUsername(string userName)
    {
        if(string.IsNullOrWhiteSpace(userName)) return false;
        if(userName.Length < 3 || userName.Length > 20) return false;
        if(userName.Any(c => !char.IsLetterOrDigit(c) && c != '_')) return false;
        return true;
    }
    
    public static void DeleteUser(string sessionKey, int userId)
    {
        if(!HasManageUsersPermission(sessionKey)) throw new Exception("Access denied.");
        
        User? user = _db.Table<User>().FirstOrDefault(u => u.Id == userId, null);
        if (user == null) throw new Exception("User not found.");
        _db.Delete(user);
    }
}