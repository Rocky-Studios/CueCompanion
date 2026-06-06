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
        AuditAction auditAction = new(AuditActionType.Create, DateTime.UtcNow, apiKey, null, $"Create user '{userName}'");
        var         r           = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess)
        {
            auditAction.SetErrorAndUpdate(r.Error!);
            return auditAction.Error;
        }

        if (Db.Table<User>().Any(u => u.UserName == userName))
        {
            auditAction.Error = "Username already exists.";
            return auditAction.Error;
        }

        if (!ValidateUsername(userName))
        {
            auditAction.Error = "Invalid username. Usernames must be 3-20 characters long and can only contain letters, digits, and underscores.";
            return auditAction.Error;
        }

        User newUser = new()
        {
            UserName     = userName,
            PasswordHash = Hash.HashPassword(password),
        };
        Db.Insert(newUser);
        auditAction.Success = true;
        auditAction.ItemID  = newUser.Id;
        auditAction.UpdateInDatabase();
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
        AuditAction auditAction = new(AuditActionType.Delete, DateTime.UtcNow, apiKey, userId, $"Delete user ID{userId}");
        var         r           = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess)
        {
            auditAction.SetErrorAndUpdate(r.Error!);
            return auditAction.Error;
        }

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userId, null);
        if (user == null)
        {
            auditAction.Error = "User not found.";
            return auditAction.Error;
        }

        Db.Delete(user);
        auditAction.Success = true;
        auditAction.UpdateInDatabase();
        return Result.Success();
    }

    public static Result AddPermissionToUser(string apiKey, int userID, int permissionID)
    {
        AuditAction auditAction = new(AuditActionType.Create, DateTime.UtcNow, apiKey, userID, $"Add permission ID{permissionID} to user ID{userID}");
        var         r           = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess)
        {
            auditAction.Error = r.Error!;
            return auditAction.Error;
        }

        UserPermission userPermission = new()
        {
            UserId       = userID,
            PermissionId = permissionID,
            Value        = true,
        };

        Db.Insert(userPermission);
        auditAction.Success = true;
        auditAction.UpdateInDatabase();
        return Result.Success();
    }

    public static Result RemovePermissionFromUser(string apiKey, int userID, int permissionID)
    {
        AuditAction auditAction = new(AuditActionType.Delete, DateTime.UtcNow, apiKey, userID, $"Remove permission ID{permissionID} from user ID{userID}");
        var         r           = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess)
        {
            auditAction.Error = r.Error!;
            return auditAction.Error;
        }


        UserPermission? userPermission = Db.Table<UserPermission>()
                                           .FirstOrDefault(up => up?.UserId == userID && up.PermissionId == permissionID, null);
        if (userPermission == null)
        {
            auditAction.SetErrorAndUpdate("User permission not found.");
            return auditAction.Error;
        }

        Db.Delete(userPermission);
        auditAction.Success = true;
        auditAction.UpdateInDatabase();
        return Result.Success();
    }

    public static Result SetLoggingInForUser(string apiKey, int userID, bool value)
    {
        AuditAction auditAction = new(AuditActionType.Update, DateTime.UtcNow, apiKey, userID, $"Set logging in for user ID{userID} to {value}");
        var         r           = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess)
        {
            auditAction.SetErrorAndUpdate("User not found.");
            return auditAction.Error;
        }

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null)
        {
            auditAction.SetErrorAndUpdate("User not found.");
            return auditAction.Error;
        }

        user.CanLogin = value;
        Db.Update(user);
        auditAction.Success = true;
        auditAction.UpdateInDatabase();
        return Result.Success();
    }

    public static Result ChangePassword(string apiKey, string currentPassword, string newPassword)
    {
        var  userResult = GetUserByApiKey(apiKey);
        User user       = userResult.Value!;
        int  userID     = user.Id;
        if (!userResult.IsSuccess) return userResult.Error ?? "User not found.";
        AuditAction auditAction = new(AuditActionType.Update, DateTime.UtcNow, apiKey, userID, $"Change password for user ID{userID}");
        try
        {
            string hashedProvidedPassword = Hash.HashPassword(currentPassword);
            if (hashedProvidedPassword != user.PasswordHash)
            {
                auditAction.SetErrorAndUpdate("Current password is incorrect.");
                return auditAction.Error;
            }

            user.PasswordHash = Hash.HashPassword(newPassword);
            Db.Update(user);
            auditAction.Success = true;
            auditAction.UpdateInDatabase();
            return Result.Success();
        }
        catch (Exception exception)
        {
            auditAction.SetErrorAndUpdate(exception.ToString());
            return auditAction.Error;
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
        AuditAction auditAction = new(AuditActionType.Update, DateTime.UtcNow, apiKey, userID, $"Rename user ID{userID} to '{newUserName}'");
        var         r           = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess)
        {
            auditAction.SetErrorAndUpdate(r.Error!);
            return auditAction.Error;
        }

        User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
        if (user == null)
        {
            auditAction.SetErrorAndUpdate("User not found.");
            return auditAction.Error;
        }

        user.UserName = newUserName;
        Db.Update(user);
        auditAction.Success = true;
        auditAction.UpdateInDatabase();
        return Result.Success();
    }

    public static Result<string> ForceChangePassword(string apiKey, int userID)
    {
        var r = HasManageUsersPermission(apiKey);
        if (!r.IsSuccess) return Result<string>.Failure(r.Error!);

        if (!r.GetValue()) return Result<string>.Failure("Access denied.");

        AuditAction auditAction = new(AuditActionType.Update, DateTime.UtcNow, apiKey, userID, $"Change password for user ID{userID}");
        try
        {
            User? user = Db.Table<User>().FirstOrDefault(u => u?.Id == userID, null);
            if (user == null)
            {
                auditAction.SetErrorAndUpdate("User not found.");
                return Result<string>.Failure(auditAction.Error);
            }

            string newPassword = RandomNumberGenerator.GetHexString(8);
            user.PasswordHash = Hash.HashPassword(newPassword);
            Db.Update(user);
            auditAction.Success = true;
            auditAction.UpdateInDatabase();
            return Result<string>.Success(newPassword);
        }
        catch (Exception exception)
        {
            auditAction.SetErrorAndUpdate(exception.ToString());
            return Result<string>.Failure(auditAction.Error);
        }
    }
}