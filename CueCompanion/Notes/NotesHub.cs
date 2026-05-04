using CueCompanion.Notes;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class NotesHub : Hub
{
    public async Task<Result> CreateNote(string apiKey, Note note)
    {
        Result r = NoteManager.CreateNote(apiKey, note);
        if (r.IsSuccess) await Clients.All.SendAsync("NoteAdded", note);
        return r;
    }

    public Task<Result<Note[]>> GetAllNotes(string apiKey)
    {
        try
        {
            return Task.FromResult(NoteManager.GetNotes(apiKey));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<Note[]>>(exception);
        }
    }

    public async Task<Result> UpdateNote(string apiKey, Note newNote)
    {
        Result r = NoteManager.UpdateNote(apiKey, newNote);
        if (r.IsSuccess) await Clients.All.SendAsync("NoteUpdated", newNote);
        return r;
    }

    public async Task<Result> DeleteNote(string apiKey, int noteID)
    {
        Result r = NoteManager.DeleteNote(apiKey, noteID);
        if (r.IsSuccess) await Clients.All.SendAsync("NoteDeleted", noteID);
        return r;
    }
}