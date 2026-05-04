using System.Security.Cryptography;
using CueCompanion.Components;
using SQLite;

namespace CueCompanion.UserManagement;

public static class UserManager
{
    private static SQLiteConnection Db => DatabaseHandler.Connection;

    private static Result<bool> HasManageUsersPermission(string apiKey)
    {
        Result<User?> r = GetUserByApiKey(apiKey);
        if (!r.IsSuccess) return r.Error ?? "Unknown session key.";
        User? user = r.Value;
        if (user == null) return "Invalid session key.";
        Permission? perm = PermissionManager.GetPermissionByName("ManageUsers");
        if (perm == null) return "Permission not found.";
        bool hasManageUsersPermission = PermissionManager.HasPermission(perm, user);
        if (!hasManageUsersPermission) return "Access denied.";
        return true;
    }

    public static Result<UserInfo[]> GetUsers(string apiKey)
    {
        Result<User?> r = GetUserByApiKey(apiKey);
        if (!r.IsSuccess) return r.Error ?? "Unknown session key.";
        User? user = r.Value;
        if (user == null) return "Invalid session key.";
        var r2 = HasManageUsersPermission(apiKey);
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

    public static Result<User?> GetUserByApiKey(string apiKey)
    {
        ApiKey? session = Db.Table<ApiKey>().FirstOrDefault(sk => sk?.Key == apiKey, null);
        if (session == null) return "Invalid session key.";
        User? user = Db.Table<User>().FirstOrDefault(u => u != null && u.Id == session.UserID, null);
        if (user == null) return "User not found for session key.";
        return user;
    }

    public static Result CreateNewUser(string apiKey, string userName, string password)
    {
        var r = HasManageUsersPermission(apiKey);
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

    public static Result DeleteUser(string apiKey, int userId)
    {
        var r = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userId, null);
        if (user == null) return "User not found.";
        Db.Delete(user);
        return Result.Success();
    }

    public static Result AddPermissionToUser(string apiKey, int userID, int permissionID)
    {
        var r = HasManageUsersPermission(apiKey);
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

    public static Result RemovePermissionFromUser(string apiKey, int userID, int permissionID)
    {
        var r = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess) return r.Error!;

        UserPermission? userPermission = Db.Table<UserPermission>()
                                           .FirstOrDefault(up => up?.UserId == userID && up.PermissionId == permissionID, null);
        if (userPermission == null) return "User permission not found.";
        Db.Delete(userPermission);
        return Result.Success();
    }

    public static Result EnableLoggingInForUser(string apiKey, int userID)
    {
        var r = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null) return "User not found.";
        user.CanLogin = true;
        Db.Update(user);
        return Result.Success();
    }

    public static Result DisableLoggingInForUser(string apiKey, int userID)
    {
        var r = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null) return "User not found.";
        user.CanLogin = false;
        Db.Update(user);
        return Result.Success();
    }

    public static Task<Result> ChangePassword(string apiKey, string currentPassword, string newPassword)
    {
        try
        {
            var userResult = GetUserByApiKey(apiKey);
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

    public static void RemoveExpiredApiKeys()
    {
        var expiredKeys = Db.Table<ApiKey>()
                            .Where(k => k.ExpiresAt < DateTime.UtcNow)
                            .ToList();
        foreach (ApiKey key in expiredKeys) Db.Delete(key);
    }

    public static Result<(User user, string apiKey)> TryConnect(string connectionName, string password)
    {
        RemoveExpiredApiKeys();
        string passwordHash = Hash.HashPassword(password);
        User? user = Db.Table<User>()
                       .FirstOrDefault(c => c?.UserName == connectionName && c.PasswordHash == passwordHash, null);
        if (user == null)
        {
            Result<(User user, string apiKey)> r =
                Result<(User user, string apiKey)>.Failure("Invalid username or password.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearApiKey"] = "true",
            };
            return r;
        }

        if (!user.CanLogin)
        {
            var r =
                Result<(User user, string apiKey)>.Failure("Login disabled.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearApiKey"] = "true",
            };
            return r;
        }

        string apiKey = GetOrAddApiKey(user);

        return (user, apiKey);
    }

    public static Result<(User user, string apiKey)> TryConnect(string apiKey)
    {
        RemoveExpiredApiKeys();
        ApiKey? apiKeyObject = Db.Table<ApiKey>()
                                 .FirstOrDefault(k => k?.Key == apiKey, null);

        if (apiKeyObject == null)
        {
            Result<(User user, string apiKey)> r =
                Result<(User user, string apiKey)>.Failure("Invalid session. Try reconnecting.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearApiKey"] = "true",
            };
            return r;
        }

        int   userID = apiKeyObject.UserID;
        User? user   = Db.Table<User>().FirstOrDefault(c => c?.Id == userID, null);

        if (user == null)
        {
            var r =
                Result<(User user, string apiKey)>.Failure("Invalid username or password.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearApiKey"] = "true",
            };
            return r;
        }

        if (!user.CanLogin)
        {
            var r =
                Result<(User user, string apiKey)>.Failure("Login disabled.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearApiKey"] = "true",
            };
            return r;
        }

        return (user, apiKey);
    }

    private static string GetOrAddApiKey(User user, bool forceNew = false)
    {
        ApiKey? existingKeyForConnection = Db.Table<ApiKey>()
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
        ApiKey apiKey = new()
        {
            UserID    = user.Id,
            Key       = key,
            IssuedAt  = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        Db.Insert(apiKey);
        return key;
    }

    public static User? GetUserById(int userId)
    {
        return Db.Table<User>().FirstOrDefault(u => u?.Id == userId, null);
    }

    public static Result RenameUser(string apiKey, int userID, string newUserName)
    {
        var r = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess) return r.Error!;

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null) return "User not found.";

        user.UserName = newUserName;
        Db.Update(user);
        return Result.Success();
    }
}