using Microsoft.AspNetCore.SignalR;

namespace CueCompanion;

public class AuthHub : Hub
{
    public async Task<ConnectionResult> ConnectAsync(string connectionName, string password)
    {
        ConnectionResult connection = DatabaseHandler.TryConnect(connectionName, password);
        return connection;
    }

    public async Task<ConnectionResult> ConnectAsyncWithKey(string connectionKey)
    {
        ConnectionResult connection = DatabaseHandler.TryConnect(connectionKey);
        return connection;
    }
}