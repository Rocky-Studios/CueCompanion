using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ShowHub : Hub
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Show? CurrentShow => ShowManager.CurrentShow;

    public async Task<Result<(Show? Show, int? CurrentCuePosition, Role[] Roles)>> GetCurrentShow(string sessionKey)
    {
        Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "ViewShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";
        Result<(Show? Show, int? CurrentCuePosition, Role[] Roles)> res = (ShowManager.CurrentShow,
            ShowManager.CurrentCuePosition, ShowManager.GetRoles());
        return res;
    }

    public async Task<Result<(Cue[] cues, CueTask[] tasks)>> GetCuesForShow(string sessionKey, int showID)
    {
        Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "ViewShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";

        Cue[] cues = ShowManager.GetCuesForShow(showID);
        CueTask[] tasks = ShowManager.GetTasksForShow(showID);

        return (cues, tasks);
    }

    public async Task<Result> StartShow(string sessionKey)
    {
        Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "ControlShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";

        if (hasPermission)
            ShowManager.StartShow();

        _ = BroadcastShowUpdate();
        return Result.Success();
    }

    public async Task BroadcastShowUpdate()
    {
        await Clients.All.SendAsync("ShowUpdated", new ShowUpdate
        {
            Show = ShowManager.CurrentShow,
            CurrentCuePosition = ShowManager.CurrentCuePosition,
            Cues = ShowManager.GetCuesForShow(ShowManager.CurrentShow?.Id ?? 0),
            Tasks = ShowManager.GetTasksForShow(ShowManager.CurrentShow?.Id ?? 0),
            Roles = ShowManager.GetRoles()
        });
    }

    public async Task<Result> NextCue(string sessionKey)
    {
        Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "ControlShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";

        if (ShowManager.CurrentShow == null)
            return "No show loaded.";

        Cue[] cues = ShowManager.GetCuesForShow(ShowManager.CurrentShow.Id);
        int currentPosition = ShowManager.CurrentCuePosition ?? 0;
        Cue? nextCue = cues.Where(c => c.Position > currentPosition).MinBy(c => c.Position);
        if (nextCue != null)
        {
            ShowManager.CurrentCuePosition = nextCue.Position;
            await BroadcastShowUpdate();
        }

        return Result.Success();
    }

    public async Task<Result> PreviousCue(string sessionKey)
    {
        Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "ControlShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";

        if (ShowManager.CurrentShow == null)
            return "No show loaded.";

        Cue[] cues = ShowManager.GetCuesForShow(ShowManager.CurrentShow.Id);
        int currentPosition = ShowManager.CurrentCuePosition ?? 0;
        Cue? previousCue = cues.Where(c => c.Position < currentPosition).MaxBy(c => c.Position);
        if (previousCue != null)
        {
            ShowManager.CurrentCuePosition = previousCue.Position;
            await BroadcastShowUpdate();
        }

        return Result.Success();
    }


    public async Task<Result> EditModeAction(string sessionKey, EditModeMethod method, JsonElement newObject,
        string objectTypeAsString, EditParameters? parameters)
    {
        try
        {
            Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "EditShow");
            if (!r.IsSuccess) return r.Error!;
            bool hasPermission = r.Value;
            if (!hasPermission) return "Access denied.";

            Type objectType = Type.GetType(objectTypeAsString)!;
            object? obj = newObject.Deserialize(objectType, _options);

            Result res = ShowManager.EditAction(method, obj, objectType, parameters);
            _ = BroadcastShowUpdate();
            return res;
        }
        catch (Exception e)
        {
            return e.ToString();
        }
    }
}