using CueCompanion.Notes;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class NotesService(ShowService showService, AuthService auth) : AuthDependantService(auth)
{
    private HubConnection? _notesHub;
    public  List<Note>     Notes = [];

    public Note[] GetCurrentlyVisibleNotes()
    {
        List<Note> currentlyVisibleNotes = [];
        foreach (Note note in Notes)
        {
            // TODO add checking
            if (currentlyVisibleNotes.Any(n => n.Id == note.Id)) continue;
            currentlyVisibleNotes.Add(note);
        }

        return currentlyVisibleNotes.ToArray();
    }

    public async Task StartAsync()
    {
        _notesHub = new HubConnectionBuilder()
                   .WithUrl($"{Program.localhostURL}/api/notes")
                   .WithAutomaticReconnect()
                   .Build();

        _notesHub.On("NoteAdded", (Note newNote) =>
                                  {
                                      Notes.Add(newNote);
                                      UpdateState();
                                  });

        _notesHub.On("NoteDelete", (int deletedNoteID) =>
                                   {
                                       Notes.RemoveAll(n => n.Id == deletedNoteID);
                                       UpdateState();
                                   });

        await _notesHub.StartAsync();
        UpdateState();
    }

    public async Task<Result<Note[]>> GetAllNotes()
    {
        Result<Note[]> notesResult =
            await InvokeWithSessionAsync(key => _notesHub!.InvokeAsync<Result<Note[]>>("GetAllNotes", key));

        if (notesResult.Value is { } notes)
        {
            Notes.Clear();
            Notes = notes.ToList();
        }

        UpdateState();
        return notesResult;
    }

    public Task<Result> CreateNote(Note note) =>
        InvokeWithSessionAsync(key => _notesHub!.InvokeAsync<Result>("CreateNote", key, note));

    public Task<Result> DeleteNote(int noteID) =>
        InvokeWithSessionAsync(key => _notesHub!.InvokeAsync<Result>("DeleteNote", key, noteID));
}