using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ShowHub : Hub
{
    public Show? CurrentShow => ShowManager.CurrentShow;

    public async Task<ShowRequestResult> GetCurrentShow(string sessionKey)
    {
        bool hasPermission = PermissionManager.UserHasPermission(sessionKey, "ViewShow", out string? error);
        return new ShowRequestResult
        {
            Success = hasPermission,
            ErrorMessage = error,
            Show = hasPermission ? ShowManager.CurrentShow : null,
            CurrentCuePosition = hasPermission ? ShowManager.CurrentCuePosition : null
        };
    }
}