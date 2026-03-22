using CueCompanion.Notes;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class NotesService : StateSubscriberService
{
    private HubConnection? _notesHub;
    public List<Note> Notes = [];

    public async Task StartAsync(string baseUrl)
    {
        _notesHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}api/notes")
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