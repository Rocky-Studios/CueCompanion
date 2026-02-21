using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion;

public class AuthHub : Hub
{
    public async Task<ConnectionResult> ConnectAsync(string connectionName, string password)
    {
        ConnectionResult connection = DatabaseHandler.TryConnect(connectionName, password);
        return connection;
    }
}