using System.Text.Json;
using CueCompanion.Services;
using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ShowHub : Hub
{
    public Show? CurrentShow => ShowManager.CurrentShow;

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public Task<Result<int?>> GetCurrentShowID(string apiKey)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "ViewShow");
            if (!r.IsSuccess) return Task.FromResult<Result<int?>>(r.Error!);
            bool hasPermission = r.Value;
            if (!hasPermission) return Task.FromResult<Result<int?>>("Access denied.");

            return Task.FromResult<Result<int?>>(ShowManager.CurrentShow?.Id);
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<int?>>(exception);
        }
    }

    public Task<Result<(Show[] shows, Role[] roles)>> GetShowsAndRoles(string apiKey)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "ViewShow");
            if (!r.IsSuccess) return Task.FromResult<Result<(Show[] shows, Role[] roles)>>(r.Error!);
            bool hasPermission = r.Value;
            if (!hasPermission) return Task.FromResult<Result<(Show[] shows, Role[] roles)>>("Access denied.");

            var roles = ShowManager.GetRoles();
            var shows = ShowManager.GetShows();
            return Task.FromResult<Result<(Show[] shows, Role[] roles)>>((shows, roles));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<(Show[] shows, Role[] roles)>>(exception);
        }
    }

    public Task<Result<ShowService.LiveInfo?>> GetLiveInfo(string apiKey)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "ViewShow");
            if (!r.IsSuccess) return Task.FromResult<Result<ShowService.LiveInfo?>>(r.Error!);
            bool hasPermission = r.Value;
            if (!hasPermission) return Task.FromResult<Result<ShowService.LiveInfo?>>("Access denied.");

            if (!ShowManager.IsShowActive) return Task.FromResult(Result<ShowService.LiveInfo?>.Success(null));
            return Task.FromResult<Result<ShowService.LiveInfo?>>(new ShowService.LiveInfo
            {
                CuePosition = ShowManager.CurrentCuePosition,
                LiveShowID  = ShowManager.CurrentShow!.Id,
            });
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<ShowService.LiveInfo?>>(exception);
        }
    }

    public Task<Result<(Cue[] cues, CueTask[] tasks)>> GetCuesForShow(string apiKey, int showID)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "ViewShow");
            if (!r.IsSuccess) return Task.FromResult<Result<(Cue[] cues, CueTask[] tasks)>>(r.Error!);
            bool hasPermission = r.Value;
            if (!hasPermission) return Task.FromResult<Result<(Cue[] cues, CueTask[] tasks)>>("Access denied.");

            var cues  = ShowManager.GetCuesForShow(showID);
            var tasks = ShowManager.GetTasksForShow(showID);

            return Task.FromResult<Result<(Cue[] cues, CueTask[] tasks)>>((cues, tasks));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<(Cue[] cues, CueTask[] tasks)>>(exception);
        }
    }

    public Task<Result> StartShow(string apiKey)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "ControlShow");
            if (!r.IsSuccess) return Task.FromResult<Result>(r.Error!);
            bool hasPermission = r.Value;
            if (hasPermission)
            {
                Result r2 = ShowManager.StartShow();
                _ = BroadcastShowUpdate();
                return Task.FromResult(r2);
            }

            return Task.FromResult<Result>("Access denied.");
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result> StopShow(string apiKey)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "ControlShow");
            if (!r.IsSuccess) return Task.FromResult<Result>(r.Error!);
            bool hasPermission = r.Value;
            if (hasPermission)
            {
                Result r2 = ShowManager.StopShow();
                _ = BroadcastShowUpdate();
                return Task.FromResult(r2);
            }

            return Task.FromResult<Result>("Access denied.");
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public async Task BroadcastShowUpdate()
    {
        await Clients.All.SendAsync("ShowUpdated", new ShowUpdate
        {
            LiveShow           = ShowManager.CurrentShow,
            CurrentCuePosition = ShowManager.CurrentCuePosition,
            Cues               = ShowManager.GetCues(),
            Tasks              = ShowManager.GetTasks(),
            Roles              = ShowManager.GetRoles(),
            Shows              = ShowManager.GetShows(),
        });
    }

    public async Task<Result> NextCue(string apiKey)
    {
        var r = PermissionManager.UserHasPermission(apiKey, "ControlShow");
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

    public async Task<Result> PreviousCue(string apiKey)
    {
        var r = PermissionManager.UserHasPermission(apiKey, "ControlShow");
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


    public Task<Result<object>> EditModeAction(string apiKey,             EditModeMethod  method, JsonElement newObject,
                                               string objectTypeAsString, EditParameters? parameters)
    {
        try
        {
            try
            {
                var r = PermissionManager.UserHasPermission(apiKey, "EditShow");
                if (!r.IsSuccess) return Task.FromResult<Result<object>>(r.Error!);
                bool hasPermission = r.Value;
                if (!hasPermission) return Task.FromResult<Result<object>>("Access denied.");

                Type    objectType = Type.GetType(objectTypeAsString)!;
                object? obj        = newObject.Deserialize(objectType, _options);

                var res = ShowManager.EditAction(method, obj!, objectType, parameters);
                _ = BroadcastShowUpdate();
                return Task.FromResult(res);
            }
            catch (Exception e)
            {
                return Task.FromResult<Result<object>>(e.ToString());
            }
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<object>>(exception);
        }
    }

    public Task<Result> SelectShow(string apiKey, int? showID)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "ControlShow");
            if (!r.IsSuccess) return Task.FromResult<Result>(r.Error!);
            bool hasPermission = r.Value;
            if (!hasPermission) return Task.FromResult<Result>("Access denied.");

            Result res = ShowManager.SelectShow(showID);
            _ = BroadcastShowUpdate();
            return Task.FromResult(res);
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }

    public Task<Result<ShowBundle>> GetShowBundle(string apiKey, int showID)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "ViewShow");
            if (!r.IsSuccess) return Task.FromResult(Result<ShowBundle>.Failure(r.Error!));
            bool hasPermission = r.Value;
            if (!hasPermission) return Task.FromResult(Result<ShowBundle>.Failure("Access denied."));

            return Task.FromResult(ShowManager.BundleShow(showID));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result<ShowBundle>>(exception);
        }
    }

    public Task<Result> AddShowFromBundle(string apiKey, ShowBundle showBundle)
    {
        try
        {
            var r = PermissionManager.UserHasPermission(apiKey, "EditShow");
            if (!r.IsSuccess) return Task.FromResult(Result.Failure(r.Error!));
            bool hasPermission = r.Value;
            if (!hasPermission) return Task.FromResult(Result.Failure("Access denied."));

            return Task.FromResult(ShowManager.AddShowFromBundle(showBundle));
        }
        catch (Exception exception)
        {
            return Task.FromException<Result>(exception);
        }
    }
}