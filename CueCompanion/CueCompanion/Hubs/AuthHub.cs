using System.Security.Claims;
using CueCompanion.Client;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class AuthHub : Hub
{
    private readonly ConnectionUserStore _store;

    public AuthHub(ConnectionUserStore store)
    {
        _store = store;
    }

    public async Task<LoginResult> Login(string username, string passkey)
    {
        bool valid = UserDatabase.Validate(username, passkey, out string role);

        if (!valid)
            return new LoginResult { Success = false, Error = "Invalid credentials" };

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        ClaimsIdentity identity = new(claims, "SignalR");
        ClaimsPrincipal principal = new(identity);

        _store.SetUser(Context.ConnectionId, principal);

        return new LoginResult { Success = true };
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _store.RemoveUser(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}