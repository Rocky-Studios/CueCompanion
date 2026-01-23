using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class AuthHub : Hub
{
    private ClientType GetRequestClientType()
    {
        string? ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();

        ClientType clientType;
        if (ip is null) clientType = ClientType.Child;
        else if (ip is "127.0.0.1") clientType = ClientType.Master; // localhost for IPv4
        else if (ip is "::1") clientType = ClientType.Master; // localhost for IPv6
        else clientType = ClientType.Child;
        return clientType;
    }

    public override async Task OnConnectedAsync()
    {
        ClientType clientType = GetRequestClientType();
        await Clients.Caller.SendAsync("ClientTypeVerification", clientType);
    }

    public async Task<UserConnectPacket> Connect(string connectionName, string connectionPasskey, Guid? secret)
    {
        UserConnectPacket packet = new();
        await Task.Run(() =>
        {
            ClientType clientType = GetRequestClientType();
            if (clientType == ClientType.Unknown)
            {
                packet.Error = "Unknown client type";
            }
            else
            {
                (Connection? connection, string? error) =
                    Program.ConnectionManager.TryConnect(connectionName, connectionPasskey, secret);
                if (error != null)
                    packet.Error = error;
                else
                    packet.Connection = connection;
            }
        });
        return packet;
    }
}