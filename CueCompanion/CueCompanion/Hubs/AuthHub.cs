using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class AuthHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        string? ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();

        ClientType clientType;
        if (ip is null) clientType = ClientType.Child;
        else if (ip is "127.0.0.1") clientType = ClientType.Master; // localhost for IPv4
        else if (ip is "::1") clientType = ClientType.Master; // localhost for IPv6
        else clientType = ClientType.Child;

        await Clients.Caller.SendAsync("ClientTypeVerification", clientType);
    }
}