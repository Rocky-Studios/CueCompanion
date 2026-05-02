using CueCompanion.Notes;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class NotesService(ShowService showService) : StateSubscriberService
{
    private HubConnection? _notesHub;
    public  List<Note>     Notes = [];
    
    public Note[] GetCurrentlyVisibleNotes()
    {
        List<Note> currentlyVisibleNotes = [];
        foreach (Note note in Notes)
        {
            if(note.CueId != showService.CurrentCue?.Id) continue;
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

        await _notesHub.StartAsync();
        UpdateState();
    }

    public async Task<Result<Note[]>> GetAllNotes(string sessionKey)
    {
        Result<Note[]> notesResult = await _notesHub.InvokeAsync<Result<Note[]>>("GetAllNotes", sessionKey);
        if (notesResult.Value is { } notes)
        {
            Notes.Clear();
            Notes.AddRange(notes);
        }

        UpdateState();
        return notesResult;
    }

    public async Task<Result> CreateNote(string sessionKey, Note note)
    {
        return await _notesHub.InvokeAsync<Result>("CreateNote", sessionKey, note);
    }
}