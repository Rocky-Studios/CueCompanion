using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ShowService : StateSubscriberService, IAsyncDisposable
{
    public int? LiveModeShowID { get; private set; }
    public bool IsLiveMode     => LiveModeShowID.HasValue;

    public int? CurrentCuePosition
    {
        get => CurrentMode switch
               {
                   Mode.Live   => LiveModeCuePosition,
                   Mode.Edit   => EditModeCuePosition,
                   Mode.Browse => BrowseModeCuePosition,
                   _           => null,
               };
        set
        {
            switch (CurrentMode)
            {
                case Mode.Live:
                    break;
                case Mode.Edit:
                    EditModeCuePosition = value;
                    break;
                case Mode.Browse:
                    BrowseModeCuePosition = value;
                    break;
            }
        }
    }

    public CueTask[] TasksForCurrentCue => Tasks.Where(t => t.CueId == CurrentCue?.Id).ToArray();

    public Cue? CurrentCue => Cues.FirstOrDefault(c => c.Position == CurrentCuePosition);

    public Show? LiveModeShow => Shows.FirstOrDefault(s => s.Id == LiveModeShowID);

    public async ValueTask DisposeAsync()
    {
        if (_showHub != null) await _showHub.DisposeAsync();
    }

    private HubConnection? _showHub;
    private int?           BrowseModeCuePosition;

    public Cue[] Cues = [];

    public  Mode CurrentMode = Mode.Live;
    private int? EditModeCuePosition;

    private int?      LiveModeCuePosition;
    public  Role[]    Roles = [];
    public  Show[]    Shows = [];
    public  CueTask[] Tasks = [];
    public  Show?     CurrentlyViewingShow;

    public async Task StartAsync(string baseUrl)
    {
        _showHub = new HubConnectionBuilder()
                  .WithUrl($"{baseUrl}api/show")
                  .WithAutomaticReconnect()
                  .Build();

        _showHub.On("ShowUpdated", (ShowUpdate update) =>
                                   {
                                       LiveModeShowID      = update.LiveShow?.Id;
                                       LiveModeCuePosition = update.CurrentCuePosition;
                                       Roles               = update.Roles;
                                       Cues                = update.Cues;
                                       Tasks               = update.Tasks;
                                       Shows               = update.Shows;

                                       // If show has been deleted or otherwise remove, switch to another
                                       if (Shows.All(s => s.Id != CurrentlyViewingShow?.Id))
                                           CurrentlyViewingShow = Shows.FirstOrDefault(s => s.Id == CurrentlyViewingShow?.Id);
                                       UpdateState();
                                   });

        await _showHub.StartAsync();
    }


    public async Task<Result<(Show[] shows, Role[] roles)>> GetShowsAndRoles(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        var r = await hub.InvokeAsync<Result<(Show[] shows, Role[] roles)>>("GetShowsAndRoles", sessionKey);
        if (r.Value is { } v)
        {
            Shows = v.shows;
            Roles = v.roles;
        }

        UpdateState();
        return r;
    }

    public async Task<Result<LiveInfo?>> GetLiveInfoAsync(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        var r                                     = await hub.InvokeAsync<Result<LiveInfo?>>("GetLiveInfo", sessionKey);
        var liveInfo                              = r.IsSuccess ? r.Value : null;
        if (liveInfo != null) LiveModeCuePosition = liveInfo.Value.CuePosition;
        UpdateState();
        return r;
    }

    public async Task<Result> GetCuesForShowAsync(string sessionKey, int showID)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;


        var r =
            await hub.InvokeAsync<Result<(Cue[], CueTask[])>>("GetCuesForShow", sessionKey, showID);
        if (!r.IsSuccess) return r.Error!;
        var (cues, tasks) = r.Value;

        Cues  = cues;
        Tasks = tasks;

        UpdateState();
        return Result.Success();
    }

    public async Task<Result> StartShowAsync(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("StartShow", sessionKey);
    }

    public async Task<Result> StopShowAsync(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("StopShow", sessionKey);
    }

    public async Task<Result> NextCueAsync(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("NextCue", sessionKey);
    }

    public async Task<Result> PreviousCueAsync(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("PreviousCue", sessionKey);
    }

    public async Task<Result<T>> SendEditModeAction<T>(string          sessionKey, EditModeMethod method, T newObject,
                                                       EditParameters? parameters = null) where T : class
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        var r = await hub.InvokeAsync<Result<object>>("EditModeAction", sessionKey, method, newObject,
                                                      typeof(T).AssemblyQualifiedName, parameters);
        T?  cast;
        var jeN = (JsonElement?)r.Value;
        if (jeN == null)
        {
            cast = null;
        }
        else
        {
            Type objectType = typeof(T);
            object obj = jeN.Value.Deserialize(objectType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            cast = obj as T;
        }

        var result = new Result<T>
        {
            Error     = r.Error,
            Value     = cast,
            IsSuccess = r.IsSuccess,
        };
        return result;
    }

    private bool TryGetConnectedHub([NotNullWhen(true)] out HubConnection? hub, out string? error)
    {
        hub = _showHub;
        if (hub is null)
        {
            error = "ShowHub connection is not established.";
            return false;
        }

        error = null;
        return true;
    }

    public async Task<Result> SelectShowAsync(string sessionKey, int? showID)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("SelectShow", sessionKey, showID);
    }

    public IEnumerable<Cue> GetCuesInShow()
    {
        return CurrentlyViewingShow is { Id: { } showID } ? Cues.Where(c => c.ShowId == showID) : [];
    }

    public struct LiveInfo
    {
        public int LiveShowID;
        public int CuePosition;
    }
}

public enum Mode
{
    Live,
    Edit,
    Browse,
}