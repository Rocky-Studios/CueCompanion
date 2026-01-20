using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class CueHub : Hub
{
    private static readonly ServerState _state = new();

    public ServerState GetState()
    {
        return _state;
    }

    public async Task UpdateEntireState(ServerState newState)
    {
        if (newState == null) return;

        // Copy incoming values into the server state (don't reassign the readonly field)
        _state.CurrentShow = newState.CurrentShow;
        _state.CurrentCueNumber = newState.CurrentCueNumber;
        _state.Connections = newState.Connections;

        await Clients.All.SendAsync("StateUpdated", _state);
    }

    public async Task UpdateCueNumber(int newCueNumber)
    {
        _state.CurrentCueNumber = newCueNumber;
        await Clients.All.SendAsync("StateUpdated", _state);
    }

    public async Task UpdateNote(string noteID, string newNoteText)
    {
        _state.CurrentCue.Notes[noteID] = newNoteText;
        await Clients.All.SendAsync("StateUpdated", _state);
    }
}