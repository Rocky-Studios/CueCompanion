using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class AuthHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        string? ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        if (ip is null) return;

        User user;

        if (ip == "127.0.0.1") // control room IP
        {
            user = new User(UserType.Master, ip);
            user.SetAllPermissions(true);
        }
        else
        {
            user = new User(UserType.Child, ip);
            user.SetAllPermissions(false);
        }

        Program.UserManager.AddUser(user);

        await Clients.Caller.SendAsync("Authenticated", user);
    }
}