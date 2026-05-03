using CueCompanion.Notes;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class NotesHub : Hub
{
    public async Task<Result> CreateNote(string sessionKey, Note note)
    {
        Result r = NoteManager.CreateNote(sessionKey, note);
        if (r.IsSuccess) await Clients.All.SendAsync("NoteAdded", note);
        return r;
    }

    public Task<Result<Note[]>> GetAllNotes(string sessionKey)
    {
        try
        {
            return Task.FromResult(NoteManager.GetNotes(sessionKey));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<Note[]>>(exception);
        }
    }

    public async Task<Result> UpdateNote(string sessionKey, Note newNote)
    {
        Result r = NoteManager.UpdateNote(sessionKey, newNote);
        if (r.IsSuccess) await Clients.All.SendAsync("NoteUpdated", newNote);
        return r;
    }

    public async Task<Result> DeleteNote(string sessionKey, int noteID)
    {
        Result r = NoteManager.DeleteNote(sessionKey, noteID);
        if (r.IsSuccess) await Clients.All.SendAsync("NoteDeleted", noteID);
        return r;
    }
}