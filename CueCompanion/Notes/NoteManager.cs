using SQLite;

namespace CueCompanion.Notes;

public static class NoteManager
{
    private static SQLiteConnection Db => DatabaseHandler.Connection;

    public static Result CreateNote(string sessionKey, Note note)
    {
        SessionKey session = Db.Table<SessionKey>().FirstOrDefault(s => s.Key == sessionKey);
        if (session == null) return "Invalid session key.";

        Db.Insert(note);
        return Result.Success();
    }

    public static Result UpdateNote(string sessionKey, Note newNote)
    {
        SessionKey session = Db.Table<SessionKey>().FirstOrDefault(s => s.Key == sessionKey);
        if (session == null) return "Invalid session key.";

        Db.Update(newNote);
        return Result.Success();
    }

    public static Result DeleteNote(string sessionKey, int noteID)
    {
        SessionKey session = Db.Table<SessionKey>().FirstOrDefault(s => s.Key == sessionKey);
        if (session == null) return "Invalid session key.";

        Db.Table<Note>().Delete(n => n.Id == noteID);
        return Result.Success();
    }

    public static Result<Note[]> GetNotes(string sessionKey)
    {
        SessionKey session = Db.Table<SessionKey>().FirstOrDefault(s => s.Key == sessionKey);
        if (session == null) return "Invalid session key.";

        Note[] notes = Db.Table<Note>().ToArray();
        return notes;
    }
}