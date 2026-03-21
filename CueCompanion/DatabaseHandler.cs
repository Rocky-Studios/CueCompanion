using System.Security.Cryptography;
using CueCompanion.Components;
using SQLite;

namespace CueCompanion;

public static class DatabaseHandler
{
    public static SQLiteConnection Connection { get; private set; }

    public static void Init()
    {
        Connection = new SQLiteConnection("data.db");
        Connection.CreateTable<Show>();
        Connection.CreateTable<User>();
        Connection.CreateTable<SessionKey>();
        Connection.CreateTable<Permission>();
        Connection.CreateTable<UserPermission>();
        Connection.CreateTable<Role>();
        Connection.CreateTable<Cue>();
        Connection.CreateTable<CueTask>();
        Connection.CreateTable<ShowRoleAssignment>();
        Connection.CreateTable<Message>();


        ShowManager.CreateDefaultRoles();
        PermissionManager.CreateDefaultPermissions();

        // Clear old show state, useful for development
        if (false)
        {
            Connection.DeleteAll<Show>();
            Connection.DeleteAll<Cue>();
            Connection.DeleteAll<CueTask>();
            Connection.DeleteAll<ShowRoleAssignment>();
        }

        // Create a default show
        if (false) ShowManager.CreateDefaultShow();


        bool hasAdmin = Connection.Table<User>().ToList().Any(c => c.UserName == "admin");
        if (!hasAdmin)
        {
            User adminUser = new()
            {
                UserName = "admin",
                PasswordHash = Hash.HashPassword("admin")
            };
            Connection.Insert(adminUser);
            Permission? adminPermission = PermissionManager.GetPermissionByName("Admin");
            Permission? manageUsersPermission = PermissionManager.GetPermissionByName("ManageUsers");
            if (adminPermission != null)
                PermissionManager.SetPermission(adminPermission, adminUser, true);
            if (manageUsersPermission != null)
                PermissionManager.SetPermission(manageUsersPermission, adminUser, true);
        }

        RemoveExpiredSessionKeys();
    }

    private static void RemoveExpiredSessionKeys()
    {
        List<SessionKey> expiredKeys = Connection.Table<SessionKey>()
            .Where(k => k.ExpiresAt < DateTime.UtcNow)
            .ToList();
        foreach (SessionKey key in expiredKeys)
        {
            Connection.Delete(key);
        }
    }

    public static Result<(User user, string sessionKey)> TryConnect(string connectionName, string password)
    {
        RemoveExpiredSessionKeys();
        string passwordHash = Hash.HashPassword(password);
        User? user = Connection.Table<User>()
            .FirstOrDefault(c => c?.UserName == connectionName && c.PasswordHash == passwordHash, null);
        if (user == null)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Invalid username or password.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true"
            };
            return r;
        }

        if (!user.CanLogin)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Login disabled.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true"
            };
            return r;
        }

        string? sessionKey = GetOrAddSessionKey(user);

        return (user, sessionKey);
    }

    public static Result<(User user, string sessionKey)> TryConnect(string sessionKey)
    {
        RemoveExpiredSessionKeys();
        SessionKey? sessionKeyObject = Connection.Table<SessionKey>()
            .FirstOrDefault(k => k?.Key == sessionKey, null);

        if (sessionKeyObject == null)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Invalid session. Try reconnecting.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true"
            };
            return r;
        }

        int userID = sessionKeyObject.UserID;
        User? user = Connection.Table<User>().FirstOrDefault(c => c?.Id == userID, null);

        if (user == null)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Invalid username or password.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true"
            };
            return r;
        }

        if (!user.CanLogin)
        {
            Result<(User user, string sessionKey)> r =
                Result<(User user, string sessionKey)>.Failure("Login disabled.");
            r.Meta = new Dictionary<string, string>
            {
                ["clearSessionKey"] = "true"
            };
            return r;
        }

        return (user, sessionKey);
    }

    private static string GetOrAddSessionKey(User user, bool forceNew = false)
    {
        SessionKey? existingKeyForConnection = Connection.Table<SessionKey>()
            .FirstOrDefault(k => k?.UserID == user.Id, null);
        if (existingKeyForConnection != null)
        {
            if (existingKeyForConnection.ExpiresAt < DateTime.UtcNow) Connection.Delete(existingKeyForConnection);

            if (forceNew)
                Connection.Delete(existingKeyForConnection);
            else
                return existingKeyForConnection.Key;
        }

        string key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        SessionKey sessionKey = new()
        {
            UserID = user.Id,
            Key = key,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        Connection.Insert(sessionKey);
        return key;
    }

    public static User? GetUserById(int userId)
    {
        return Connection.Table<User>().FirstOrDefault(u => u?.Id == userId, null);
    }
}