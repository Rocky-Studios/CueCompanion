using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion;

public class AuthHub : Hub
{
    public async Task<Connection?> ConnectAsync(string connectionName, string password)
    {
        Connection? connection = DatabaseHandler.TryConnect(connectionName, password);
        return connection;
    }
}