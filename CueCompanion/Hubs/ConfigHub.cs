using CueCompanion.UserManagement;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ConfigHub : Hub
{
    public async Task<Result<ApiKey[]>> GetApiKeys(string apikey) => await Task.FromResult(UserManager.GetApiKeys(apikey));
}