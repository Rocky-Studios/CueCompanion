using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ShowHub : Hub
{
    public Show? CurrentShow => ShowManager.CurrentShow;

    public async Task<Show?> GetCurrentShow()
    {
        return CurrentShow;
    }
}