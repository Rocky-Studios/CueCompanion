using System.Security.Cryptography;
using System.Text;
using SQLite;

namespace CueCompanion;

public static class DatabaseHandler
{
    private static SQLiteConnection _db;

    public static void Init()
    {
        _db = new SQLiteConnection("data.db");
        _db.CreateTable<Show>();
        _db.CreateTable<Connection>();
        _db.CreateTable<SessionKey>();


        bool hasAdmin = _db.Table<Connection>().ToList().Any(c => c.ConnectionName == "admin");
        if (!hasAdmin)
        {
            Connection adminConnection = new()
            {
                ConnectionName = "admin",
                PasswordHash = HashPassword("admin")
            };
            _db.Insert(adminConnection);
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

    public static ConnectionResult TryConnect(string connectionName, string password)
    {
        string passwordHash = HashPassword(password);
        Connection? connection = _db.Table<Connection>()
            .FirstOrDefault(c => c?.ConnectionName == connectionName && c.PasswordHash == passwordHash, null);
        string? errorMessage = null;
        string? sessionKey = null;
        if (connection == null)
            errorMessage = "Invalid connection name or password.";
        else
        {
            sessionKey = GetOrAddSessionKey(connection);
        }

        return new ConnectionResult
        {
            Connection = connection,
            ErrorMessage = errorMessage,
            SessionKey = sessionKey
        };
    }

    public static ConnectionResult TryConnect(string connectionKey)
    {
        SessionKey? sessionKey = _db.Table<SessionKey>()
            .FirstOrDefault(k => k?.Key == connectionKey, null);

        Connection? connection = null;
        string? errorMessage = null;
        if (sessionKey == null)
        {
            errorMessage = "Invalid connection key.";
        }
        else
        {
            int connectionId = sessionKey.ConnectionId;
            connection = _db.Table<Connection>()
                .FirstOrDefault(c => c?.Id == connectionId, null);
        }

        if (connection == null)
            errorMessage = "No connection found.";

        return new ConnectionResult
        {
            Connection = connection,
            ErrorMessage = errorMessage,
            SessionKey = connectionKey
        };
    }

    private static string GetOrAddSessionKey(Connection connection, bool forceNew = false)
    {
        SessionKey? existingKeyForConnection = _db.Table<SessionKey>()
            .FirstOrDefault(k => k?.ConnectionId == connection.Id, null);
        if (existingKeyForConnection != null)
        {
            if (existingKeyForConnection.ExpiresAt < DateTime.UtcNow) _db.Delete(existingKeyForConnection);

            if (forceNew)
                _db.Delete(existingKeyForConnection);
            else
                return existingKeyForConnection.Key;
        }

        string key = "key";
        SessionKey sessionKey = new()
        {
            ConnectionId = connection.Id,
            Key = key,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        _db.Insert(sessionKey);
        return key;
    }
}