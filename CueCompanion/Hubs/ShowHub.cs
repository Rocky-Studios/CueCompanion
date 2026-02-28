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

    public async Task StartShow(string sessionKey)
    {
        bool hasPermission = PermissionManager.UserHasPermission(sessionKey, "ControlShow", out string? error);
        if (hasPermission)
            ShowManager.StartShow();

        _ = BroadcastShowUpdate();
    }

    public async Task BroadcastShowUpdate()
    {
        await Clients.All.SendAsync("ShowUpdated", new ShowUpdate
        {
            Show = ShowManager.CurrentShow,
            CurrentCuePosition = ShowManager.CurrentCuePosition,
            Cues = new CueRequestResult
            {
                Success = true,
                Cues = ShowManager.GetCuesForShow(ShowManager.CurrentShow?.Id ?? 0),
                Tasks = ShowManager.GetTasksForShow(ShowManager.CurrentShow?.Id ?? 0)
            }
        });
    }
}