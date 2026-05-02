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
            if (note.Assignment != NoteAssignment.AllShows)
                if (note.Assignment != NoteAssignment.AllCues)
                    if (showService.CurrentCue is { } currentCue)
                        if (note.Assignment is NoteAssignment.SingleCue or NoteAssignment.CueList)

                            // Skip note if the cues list does not include the current cue
                            if (!note.CueList.Contains(currentCue.Id))
                                continue;

            // TODO add show-specific cues
            // Remove any duplicates
            if (currentlyVisibleNotes.Any(n => n.Id == note.Id)) continue;
            currentlyVisibleNotes.Add(note);
        }

        return currentlyVisibleNotes.ToArray();
    }

    public async Task StartAsync(string baseurl)
    {
        _notesHub = new HubConnectionBuilder()
                   .WithUrl($"{baseurl}api/notes")
                   .WithAutomaticReconnect()
                   .Build();

        _notesHub.On("NoteAdded", (Note newNote) =>
                                  {
                                      Notes.Add(newNote);
                                      UpdateState();
                                  });

        _notesHub.On("NoteUpdated", (Note newNote) =>
                                    {
                                        int id = newNote.Id;
                                        Notes.Remove(Notes.FirstOrDefault(n => n.Id == id));
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
        var notesResult =
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

    public Task<Result> UpdateNote(Note note) =>
        InvokeWithSessionAsync(key => _notesHub!.InvokeAsync<Result>("UpdateNote", key, note));

    public Task<Result> DeleteNote(int noteID) =>
        InvokeWithSessionAsync(key => _notesHub!.InvokeAsync<Result>("DeleteNote", key, noteID));
}