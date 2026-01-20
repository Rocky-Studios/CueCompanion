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

        await Clients.Caller.SendAsync("UserConnected", user);
    }

    public Task<ConnectionsPacket> GetConnections(User user)
    {
        ConnectionsPacket returnValue = new();
        if (user.UserId != Program.UserManager.GetUser(user.IPAddress)?.UserId)
        {
            returnValue.Error = "User not recognized.";
            return Task.FromResult(returnValue);
        }

        if (user.UserType != UserType.Master)
        {
            returnValue.Error = "Insufficient permissions to get connections.";
            return Task.FromResult(returnValue);
        }

        Connection[] connections = Program.ConnectionManager.GetConnections();
        returnValue.Connections = connections;
        return Task.FromResult(returnValue);
    }

    public Task<(Connection? connection, Exception? error)> Connect(User user, string connectionName,
        string connectionKey)
    {
        (Connection? connection, Exception? error) =
            Program.ConnectionManager.TryConnect(connectionName, connectionKey);
        return Task.FromResult((connection, error));
    }
}