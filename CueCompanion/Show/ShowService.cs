using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ShowService : StateSubscriberService, IAsyncDisposable
{
    public Show? CurrentShow { get; private set; }

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
    public  CueTask[] Tasks = [];

    public async Task StartAsync(string baseUrl)
    {
        _showHub = new HubConnectionBuilder()
                  .WithUrl($"{baseUrl}api/show")
                  .WithAutomaticReconnect()
                  .Build();

        _showHub.On("ShowUpdated", (ShowUpdate update) =>
                                   {
                                       CurrentShow         = update.Show;
                                       LiveModeCuePosition = update.CurrentCuePosition;
                                       Roles               = update.Roles;
                                       Cues                = update.Cues;
                                       Tasks               = update.Tasks;
                                       UpdateState();
                                   });

        await _showHub.StartAsync();
    }

    public async Task<Result<int?>> GetCurrentShowIDAsync(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        var r = await hub.InvokeAsync<Result<int?>>("GetCurrentShowID", sessionKey);
        if (!r.IsSuccess) return r.Error!;

        return r.Value;
    }

    public async Task<Result<Show?>> GetShowAsync(string sessionKey, int showID)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        var r = await hub.InvokeAsync<Result<(Show? Show, int? CurrentCuePosition, Role[] Roles)>>("GetShow", sessionKey, showID);

        Show? show               = r.Value.Show;
        int?  currentCuePosition = r.Value.CurrentCuePosition;
        var   roles              = r.Value.Roles;

        CurrentShow         = show;
        LiveModeCuePosition = currentCuePosition;
        Roles               = roles;
        UpdateState();
        return show;
    }

    public async Task<Result> GetCuesForShowAsync(string sessionKey, int showID)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;


        Result<(Cue[], CueTask[])> r =
            await hub.InvokeAsync<Result<(Cue[], CueTask[])>>("GetCuesForShow", sessionKey, showID);
        if (!r.IsSuccess) return r.Error!;
        (Cue[] cues, CueTask[] tasks) = r.Value;

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
}

public enum Mode
{
    Live,
    Edit,
    Browse
}