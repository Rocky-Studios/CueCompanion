using System.Security.Cryptography;
using CueCompanion.Components;
using SQLite;

namespace CueCompanion.UserManagement;

public static class UserManager
{
    private static SQLiteConnection Db => DatabaseHandler.Connection;

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

        var            users     = Db.Table<User>().ToArray();
        List<UserInfo> userInfos = [];
        foreach (User u in users)
        {
            Result<Permission[]> perms = PermissionManager.GetPermissionsForUser(u);
            string?              error = null;
            Permission[]         p     = perms.GetValue(s => { error = s; });
            userInfos.Add(new UserInfo
            {
                UserID      = u.Id,
                UserName    = u.UserName,
                Permissions = p,
                CanLogin    = u.CanLogin,
            });
            if (error != null)
                return $"Error retrieving permissions for user {u.UserName}: {error}";
        }

        return Result<UserInfo[]>.Success(userInfos.ToArray());
    }

    public static Result<User?> GetUserBySessionKey(string sessionKey)
    {
        SessionKey? session = Db.Table<SessionKey>().FirstOrDefault(sk => sk?.Key == sessionKey, null);
        if (session == null) return "Invalid session key.";
        User? user = Db.Table<User>().FirstOrDefault(u => u != null && u.Id == session.UserID, null);
        if (user == null) return "User not found for session key.";
        return user;
    }

    public static Result CreateNewUser(string sessionKey, string userName, string password)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        if (Db.Table<User>().Any(u => u.UserName == userName))
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
            UserName     = userName,
            PasswordHash = Hash.HashPassword(password),
        };
        Db.Insert(newUser);
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

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userId, null);
        if (user == null) return "User not found.";
        Db.Delete(user);
        return Result.Success();
    }

    public static Result AddPermissionToUser(string sessionKey, int userID, int permissionID)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        UserPermission userPermission = new()
        {
            UserId       = userID,
            PermissionId = permissionID,
            Value        = true,
        };

        Db.Insert(userPermission);
        return Result.Success();
    }

    public static Result RemovePermissionFromUser(string sessionKey, int userID, int permissionID)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        UserPermission? userPermission = Db.Table<UserPermission>()
                                           .FirstOrDefault(up => up?.UserId == userID && up.PermissionId == permissionID, null);
        if (userPermission == null) return "User permission not found.";
        Db.Delete(userPermission);
        return Result.Success();
    }

    public static Result EnableLoggingInForUser(string sessionKey, int userID)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null) return "User not found.";
        user.CanLogin = true;
        Db.Update(user);
        return Result.Success();
    }

    public static Result DisableLoggingInForUser(string sessionKey, int userID)
    {
        Result<bool> r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null) return "User not found.";
        user.CanLogin = false;
        Db.Update(user);
        return Result.Success();
    }

    public static Task<Result> ChangePassword(string sessionKey, string currentPassword, string newPassword)
    {
        try
        {
            var userResult = GetUserBySessionKey(sessionKey);
            if (!userResult.IsSuccess) return Task.FromResult<Result>(userResult.Error ?? "User not found.");

            string hashedProvidedPassword = Hash.HashPassword(currentPassword);
            User   user                   = userResult.Value!;
            if (hashedProvidedPassword != user.PasswordHash)
                return Task.FromResult<Result>("Current password is incorrect.");

            user.PasswordHash = Hash.HashPassword(newPassword);
            Db.Update(user);
            return Task.FromResult(Result.Success());
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public static void RemoveExpiredSessionKeys()
    {
        var expiredKeys = Db.Table<SessionKey>()
                            .Where(k => k.ExpiresAt < DateTime.UtcNow)
                            .ToList();
        foreach (SessionKey key in expiredKeys) Db.Delete(key);
    }

    public static Result<(User user, string sessionKey)> TryConnect(string connectionName, string password)
    {
        RemoveExpiredSessionKeys();
        string passwordHash = Hash.HashPassword(password);
        User? user = Db.Table<User>()
                       .FirstOrDefault(c => c?.UserName == connectionName && c.PasswordHash == passwordHash, null);
        if (user == null)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Invalid username or password.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true",
            };
            return r;
        }

        if (!user.CanLogin)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Login disabled.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true",
            };
            return r;
        }

        string sessionKey = GetOrAddSessionKey(user);

        return (user, sessionKey);
    }

    public static Result<(User user, string sessionKey)> TryConnect(string sessionKey)
    {
        RemoveExpiredSessionKeys();
        SessionKey? sessionKeyObject = Db.Table<SessionKey>()
                                         .FirstOrDefault(k => k?.Key == sessionKey, null);

        if (sessionKeyObject == null)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Invalid session. Try reconnecting.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true",
            };
            return r;
        }

        int   userID = sessionKeyObject.UserID;
        User? user   = Db.Table<User>().FirstOrDefault(c => c?.Id == userID, null);

        if (user == null)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Invalid username or password.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true",
            };
            return r;
        }

        if (!user.CanLogin)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Login disabled.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true",
            };
            return r;
        }

        return (user, sessionKey);
    }

    private static string GetOrAddSessionKey(User user, bool forceNew = false)
    {
        SessionKey? existingKeyForConnection = Db.Table<SessionKey>()
                                                 .FirstOrDefault(k => k?.UserID == user.Id, null);
        if (existingKeyForConnection != null)
        {
            if (existingKeyForConnection.ExpiresAt < DateTime.UtcNow) Db.Delete(existingKeyForConnection);

            if (forceNew)
                Db.Delete(existingKeyForConnection);
            else
                return existingKeyForConnection.Key;
        }

        string key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        SessionKey sessionKey = new()
        {
            UserID    = user.Id,
            Key       = key,
            IssuedAt  = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        Db.Insert(sessionKey);
        return key;
    }

    public static User? GetUserById(int userId)
    {
        return Db.Table<User>().FirstOrDefault(u => u?.Id == userId, null);
    }

    public static Result RenameUser(string sessionKey, int userID, string newUserName)
    {
        var r = HasManageUsersPermission(sessionKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null) return "User not found.";

        user.UserName = newUserName;
        Db.Update(user);
        return Result.Success();
    }
}