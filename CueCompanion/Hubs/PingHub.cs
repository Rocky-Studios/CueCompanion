namespace CueCompanion.Hubs;

using Microsoft.AspNetCore.SignalR;

public class PingHub : Hub
{
    public Task<string> Ping()
    {
        return Task.FromResult($"Hub Pong at {DateTime.Now:T}");
    }
}
