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
    }
}