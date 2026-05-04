using SQLite;

namespace CueCompanion.Notes;

public static class NoteManager
{
    private static SQLiteConnection Db => DatabaseHandler.Connection;

    public static Result CreateNote(string apiKey, Note note)
    {
        ApiKey api = Db.Table<ApiKey>().FirstOrDefault(s => s.Key == apiKey);
        if (api == null) return "Invalid api key.";

        Db.Insert(note);
        return Result.Success();
    }

    public static Result UpdateNote(string apiKey, Note newNote)
    {
        ApiKey api = Db.Table<ApiKey>().FirstOrDefault(s => s.Key == apiKey);
        if (api == null) return "Invalid api key.";

        Db.Update(newNote);
        return Result.Success();
    }

    public static Result DeleteNote(string apiKey, int noteID)
    {
        ApiKey api = Db.Table<ApiKey>().FirstOrDefault(s => s.Key == apiKey);
        if (api == null) return "Invalid api key.";

        Db.Table<Note>().Delete(n => n.Id == noteID);
        return Result.Success();
    }

    public static Result<Note[]> GetNotes(string apiKey)
    {
        ApiKey api = Db.Table<ApiKey>().FirstOrDefault(s => s.Key == apiKey);
        if (api == null) return "Invalid api key.";

        Note[] notes = Db.Table<Note>().ToArray();
        return notes;
    }
}