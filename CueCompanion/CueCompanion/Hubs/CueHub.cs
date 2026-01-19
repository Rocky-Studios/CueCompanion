using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class CueHub : Hub
{
    private static ServerState _state = new() { CurrentCueNumber = 0 };

    public ServerState GetState() => _state;

    public async Task UpdateCueNumber(int newCueNumber)
    {
        _state.CurrentCueNumber = newCueNumber;
        await Clients.All.SendAsync("StateUpdated", _state);
    }
}