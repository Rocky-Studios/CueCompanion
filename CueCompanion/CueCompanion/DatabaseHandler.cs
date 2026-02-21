using CueCompanion.Client;
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
        // Implement your password hashing logic here
        return password; // Placeholder, replace with actual hash
    }
}