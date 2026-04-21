using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ShowHub : Hub
{
    public Show? CurrentShow => ShowManager.CurrentShow;

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Result<int?>> GetCurrentShowID(string sessionKey)
    {
        var r = PermissionManager.UserHasPermission(sessionKey, "ViewShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";

        return ShowManager.CurrentShow?.Id;
    }

    public async Task<Result<(Show? Show, int? CurrentCuePosition, Role[] Roles)>> GetShow(string sessionKey, int showID)
    {
        Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "ViewShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";
        Result<(Show? Show, int? CurrentCuePosition, Role[] Roles)> res;
        int?                                                        liveShowID = ShowManager.CurrentShow?.Id;
        if (showID == liveShowID)
            res = (ShowManager.CurrentShow,
                   ShowManager.CurrentCuePosition, ShowManager.GetRoles());
        else
            res = (ShowManager.GetShowById(showID), null, ShowManager.GetRoles());

        return res;
    }

    public async Task<Result<(Cue[] cues, CueTask[] tasks)>> GetCuesForShow(string sessionKey, int showID)
    {
        Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "ViewShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";

        var       cues  = ShowManager.GetCuesForShow(showID);
        CueTask[] tasks = ShowManager.GetTasksForShow(showID);

        return (cues, tasks);
    }

    public async Task<Result> StartShow(string sessionKey)
    {
        Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "ControlShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (hasPermission)
        {
            Result r2 = ShowManager.StartShow();
            _ = BroadcastShowUpdate();
            return r2;
        }

        return "Access denied.";
    }

    public async Task<Result> StopShow(string sessionKey)
    {
        var r = PermissionManager.UserHasPermission(sessionKey, "ControlShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (hasPermission)
        {
            Result r2 = ShowManager.StopShow();
            _ = BroadcastShowUpdate();
            return r2;
        }

        return "Access denied.";
    }

    public async Task BroadcastShowUpdate()
    {
        await Clients.All.SendAsync("ShowUpdated", new ShowUpdate
        {
            Show               = ShowManager.CurrentShow,
            CurrentCuePosition = ShowManager.CurrentCuePosition,
            Cues               = ShowManager.GetCuesForShow(ShowManager.CurrentShow?.Id  ?? 0),
            Tasks              = ShowManager.GetTasksForShow(ShowManager.CurrentShow?.Id ?? 0),
            Roles              = ShowManager.GetRoles(),
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

        var  cues            = ShowManager.GetCuesForShow(ShowManager.CurrentShow.Id);
        int  currentPosition = ShowManager.CurrentCuePosition ?? 0;
        Cue? nextCue         = cues.Where(c => c.Position > currentPosition).MinBy(c => c.Position);
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

        var  cues            = ShowManager.GetCuesForShow(ShowManager.CurrentShow.Id);
        int  currentPosition = ShowManager.CurrentCuePosition ?? 0;
        Cue? previousCue     = cues.Where(c => c.Position < currentPosition).MaxBy(c => c.Position);
        if (previousCue != null)
        {
            ShowManager.CurrentCuePosition = previousCue.Position;
            await BroadcastShowUpdate();
        }

        return Result.Success();
    }


    public async Task<Result<object>> EditModeAction(string sessionKey,         EditModeMethod  method, JsonElement newObject,
                                                     string objectTypeAsString, EditParameters? parameters)
    {
        try
        {
            Result<bool> r = PermissionManager.UserHasPermission(sessionKey, "EditShow");
            if (!r.IsSuccess) return r.Error!;
            bool hasPermission = r.Value;
            if (!hasPermission) return "Access denied.";

            Type    objectType = Type.GetType(objectTypeAsString)!;
            object? obj        = newObject.Deserialize(objectType, _options);

            var res = ShowManager.EditAction(method, obj, objectType, parameters);
            _ = BroadcastShowUpdate();
            return res;
        }
        catch (Exception e)
        {
            return e.ToString();
        }
    }

    public async Task<Result> SelectShow(string sessionKey, int? showID)
    {
        var r = PermissionManager.UserHasPermission(sessionKey, "ControlShow");
        if (!r.IsSuccess) return r.Error!;
        bool hasPermission = r.Value;
        if (!hasPermission) return "Access denied.";

        Result res = ShowManager.SelectShow(showID);
        _ = BroadcastShowUpdate();
        return res;
    }
}