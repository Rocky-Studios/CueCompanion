using SQLite;

namespace CueCompanion.Notes;

public static class NoteManager
{
    private static SQLiteConnection _db => DatabaseHandler.Connection;

    public static Result CreateNote(string sessionKey, Note note)
    {
        SessionKey session = _db.Table<SessionKey>().FirstOrDefault(s => s.Key == sessionKey);
        if (session == null) return "Invalid session key.";

        _db.Insert(note);
        return Result.Success();
    }

    public static Result<Note[]> GetNotes(string sessionKey)
    {
        SessionKey session = _db.Table<SessionKey>().FirstOrDefault(s => s.Key == sessionKey);
        if (session == null) return "Invalid session key.";

        Note[] notes = _db.Table<Note>().ToArray();
        return notes;
    }
}