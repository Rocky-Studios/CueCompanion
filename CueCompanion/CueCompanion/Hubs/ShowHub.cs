using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ShowHub : Hub
{
    private static readonly ShowState _state = ShowState.CreateSampleState();

    public ShowResponsePacket GetShow(Connection connection)
    {
        (bool status, string? message) = Program.ConnectionManager.IsConnectionValid(connection);
        ShowResponsePacket response = new();
        if (status)
            response.State = _state;
        response.ErrorMessage = message;
        return response;
    }

    public async Task UpdateEntireState(Show newShow, int newCueNumber)
    {
        // Copy incoming values into the server state (don't reassign the readonly field)
        _state.CurrentShow = newShow;
        _state.CurrentCueNumber = newCueNumber;

        await Clients.All.SendAsync("StateUpdated", _state);
    }
}