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

    public async Task<CueRequestResult> GetCuesForShow(string sessionKey, int showID)
    {
        bool hasPermission = PermissionManager.UserHasPermission(sessionKey, "ViewShow", out string? error);
        if (!hasPermission)
            return new CueRequestResult
            {
                Success = false,
                ErrorMessage = error
            };

        Cue[] cues = ShowManager.GetCuesForShow(showID);
        CueTask[] tasks = ShowManager.GetTasksForShow(showID);

        return new CueRequestResult
        {
            Success = true,
            Cues = cues,
            Tasks = tasks
        };
    }
}