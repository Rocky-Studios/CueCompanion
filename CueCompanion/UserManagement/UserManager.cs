using CueCompanion.Components;
using SQLite;

namespace CueCompanion.UserManagement;

public static class UserManager
{
    private static SQLiteConnection _db => DatabaseHandler.Connection;

    private static Result<bool> HasManageUsersPermission(string sessionKey)
    {
        Result<User?> r = GetUserBySessionKey(sessionKey);
        if (!r.IsSuccess) return r.Error ?? "Unknown session key.";
        User? user = r.Value;
        if (user == null) return "Invalid session key.";
        Permission? perm = PermissionManager.GetPermissionByName("ManageUsers");
        if (perm == null) return "Permission not found.";
        bool hasManageUsersPermission = PermissionManager.HasPermission(perm, user);
        if (!hasManageUsersPermission) return "Access denied.";
        return true;
    }

    public static Result<UserInfo[]> GetUsers(string sessionKey)
    {
        Result<User?> r = GetUserBySessionKey(sessionKey);
        if (!r.IsSuccess) return r.Error ?? "Unknown session key.";
        User? user = r.Value;
        if (user == null) return "Invalid session key.";
        Result<bool> r2 = HasManageUsersPermission(sessionKey);
        if (!r2.IsSuccess) return r.Error!;

        User[] users = _db.Table<User>().ToArray();
        List<UserInfo> userInfos = [];
        foreach (User u in users)
        {
            Result<Permission[]> perms = PermissionManager.GetPermissionsForUser(u);
            string? error = null;
            Permission[] p = perms.GetValue(s => { error = s; });
            userInfos.Add(new UserInfo
            {
                UserID = u.Id,
                UserName = u.UserName,
                Permissions = p,
                CanLogin = u.CanLogin
            });
            if (error != null)
                return $"Error retrieving permissions for user {u.UserName}: {error}";
        }

        return Result<UserInfo[]>.Success(userInfos.ToArray());
    }

    public static Result<User?> GetUserBySessionKey(string sessionKey)
    {
        SessionKey? session = _db.Table<SessionKey>().FirstOrDefault(sk => sk?.Key == sessionKey, null);
        if (session == null) return "Invalid session key.";
        User? user = _db.Table<User>().FirstOrDefault(u => u != null && u.Id == session.UserID, null);
        if (user == null) return "User not found for session key.";
        return user;
    }

    public static Result CreateNewUser(string sessionKey, string userName, string password)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        if (_db.Table<User>().Any(u => u.UserName == userName))
        {
            return "Username already exists.";
        }

        if (!ValidateUsername(userName))
        {
            return
                "Invalid username. Usernames must be 3-20 characters long and can only contain letters, digits, and underscores.";
        }

        User newUser = new()
        {
            UserName = userName,
            PasswordHash = Hash.HashPassword(password),
        };
        _db.Insert(newUser);
        return Result.Success();
    }

    private static bool ValidateUsername(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return false;
        if (userName.Length < 3 || userName.Length > 20) return false;
        if (userName.Any(c => !char.IsLetterOrDigit(c) && c != '_')) return false;
        return true;
    }

    public static Result DeleteUser(string sessionKey, int userId)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = _db.Table<User>().FirstOrDefault(u => u?.Id == userId, null);
        if (user == null) return "User not found.";
        _db.Delete(user);
        return Result.Success();
    }

    public static Result AddPermissionToUser(string sessionKey, int userID, int permissionID)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        UserPermission userPermission = new()
        {
            UserId = userID,
            PermissionId = permissionID,
            Value = true
        };

        _db.Insert(userPermission);
        return Result.Success();
    }

    public static Result RemovePermissionFromUser(string sessionKey, int userID, int permissionID)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        UserPermission? userPermission = _db.Table<UserPermission>()
            .FirstOrDefault(up => up?.UserId == userID && up.PermissionId == permissionID, null);
        if (userPermission == null) return "User permission not found.";
        _db.Delete(userPermission);
        return Result.Success();
    }

    public static Result EnableLoggingInForUser(string sessionKey, int userID)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = _db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null) return "User not found.";
        user.CanLogin = true;
        _db.Update(user);
        return Result.Success();
    }

    public static Result DisableLoggingInForUser(string sessionKey, int userID)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = _db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null) return "User not found.";
        user.CanLogin = false;
        _db.Update(user);
        return Result.Success();
    }

    public static async Task<Result> ChangePassword(string sessionKey, string currentPassword, string newPassword)
    {
        Result<User?> userResult = GetUserBySessionKey(sessionKey);
        if (!userResult.IsSuccess) return userResult.Error ?? "User not found.";

        string hashedProvidedPassword = Hash.HashPassword(currentPassword);
        User user = userResult.Value!;
        if (hashedProvidedPassword != user.PasswordHash)
            return "Current password is incorrect.";

        user.PasswordHash = Hash.HashPassword(newPassword);
        _db.Update(user);
        return Result.Success();
    }
}