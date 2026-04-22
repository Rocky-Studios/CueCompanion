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

    public async Task<Result<Note[]>> GetAllNotes(string sessionKey)
    {
        return NoteManager.GetNotes(sessionKey);
    }
}