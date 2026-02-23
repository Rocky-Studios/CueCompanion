using System.Security.Cryptography;
using System.Text;
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

        // Ensure permissions exist before seeding any users
        PermissionManager.CreateDefaultPermissions();

        bool hasAdmin = Connection.Table<User>().ToList().Any(c => c.UserName == "admin");
        if (!hasAdmin)
        {
            User adminUser = new()
            {
                UserName = "admin",
                PasswordHash = HashPassword("admin")
            };
            Connection.Insert(adminUser);
            Permission? adminPermission = PermissionManager.GetPermissionByName("Admin");
            Permission? manageUsersPermission = PermissionManager.GetPermissionByName("ManageUsers");
            if (adminPermission != null)
                PermissionManager.SetPermission(adminPermission, adminUser, true);
            if (manageUsersPermission != null)
                PermissionManager.SetPermission(manageUsersPermission, adminUser, true);
        }
    }

    private static string HashPassword(string password)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] salt = "ReecesPieces".Select(c => (byte)c).ToArray();

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
        Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
        Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

        byte[] hash = sha256.ComputeHash(saltedPassword);
        byte[] hashWithSalt = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, hashWithSalt, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, hashWithSalt, salt.Length, hash.Length);

        return Convert.ToBase64String(hashWithSalt);
    }

    public static UserConnectionResult TryConnect(string connectionName, string password)
    {
        string passwordHash = HashPassword(password);
        User? connection = Connection.Table<User>()
            .FirstOrDefault(c => c?.UserName == connectionName && c.PasswordHash == passwordHash, null);
        string? errorMessage = null;
        string? sessionKey = null;
        if (connection == null)
            errorMessage = "Invalid connection name or password.";
        else
        {
            sessionKey = GetOrAddSessionKey(connection);
        }

        return new UserConnectionResult
        {
            User = connection,
            ErrorMessage = errorMessage,
            SessionKey = sessionKey
        };
    }

    public static UserConnectionResult TryConnect(string connectionKey)
    {
        SessionKey? sessionKey = Connection.Table<SessionKey>()
            .FirstOrDefault(k => k?.Key == connectionKey, null);

        User? connection = null;
        string? errorMessage = null;
        if (sessionKey == null)
        {
            errorMessage = "Invalid connection key.";
        }
        else
        {
            int connectionId = sessionKey.ConnectionId;
            connection = Connection.Table<User>()
                .FirstOrDefault(c => c?.Id == connectionId, null);
        }

        if (connection == null)
            errorMessage = "No connection found.";

        return new UserConnectionResult
        {
            User = connection,
            ErrorMessage = errorMessage,
            SessionKey = connectionKey
        };
    }

    private static string GetOrAddSessionKey(User user, bool forceNew = false)
    {
        SessionKey? existingKeyForConnection = Connection.Table<SessionKey>()
            .FirstOrDefault(k => k?.ConnectionId == user.Id, null);
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
            ConnectionId = user.Id,
            Key = key,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        Connection.Insert(sessionKey);
        return key;
    }
    
    public static User GetUserById(int userId)
    {
        return Connection.Table<User>().FirstOrDefault(u => u.Id == userId, null);
    }
}