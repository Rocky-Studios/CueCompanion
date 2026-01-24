using System.Security.Claims;

namespace CueCompanion;

public class ConnectionUserStore
{
    private readonly Dictionary<string, ClaimsPrincipal> _users = new();

    public void SetUser(string connectionId, ClaimsPrincipal user)
    {
        _users[connectionId] = user;
    }

    public ClaimsPrincipal? GetUser(string connectionId)
    {
        return _users.TryGetValue(connectionId, out ClaimsPrincipal? user) ? user : null;
    }

    public void RemoveUser(string connectionId)
    {
        _users.Remove(connectionId);
    }
}