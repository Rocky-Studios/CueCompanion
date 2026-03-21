using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ShowService : StateSubscriberService
{
    private HubConnection? _showHub;
    private int? BrowseModeCuePosition;

    public Cue[] Cues = [];

    public Mode CurrentMode = Mode.Live;
    private int? EditModeCuePosition;

    private int? LiveModeCuePosition;
    public Role[] Roles = [];
    public CueTask[] Tasks = [];
    public Show? CurrentShow { get; private set; }

    public int? CurrentCuePosition
    {
        get => CurrentMode switch
        {
            Mode.Live => LiveModeCuePosition,
            Mode.Edit => EditModeCuePosition,
            Mode.Browse => BrowseModeCuePosition,
            _ => null
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

    public async Task StartAsync(string baseUrl)
    {
        _showHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}api/show")
            .WithAutomaticReconnect()
            .Build();

        _showHub.On("ShowUpdated", (ShowUpdate update) =>
        {
            CurrentShow = update.Show;
            LiveModeCuePosition = update.CurrentCuePosition;
            Roles = update.Roles;
            UpdateState();
        });

        await _showHub.StartAsync();
    }

    public async Task<Result<Show?>> GetCurrentShowAsync(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        Result<(Show?, int?, Role[])> r =
            await hub.InvokeAsync<Result<(Show?, int?, Role[])>>("GetCurrentShow", sessionKey);
        if (!r.IsSuccess) return r.Error!;
        (Show? show, int? currentCuePosition, Role[] roles) = r.Value;

        CurrentShow = show;
        LiveModeCuePosition = currentCuePosition;
        Roles = roles;

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

        Cues = cues;
        Tasks = tasks;

        UpdateState();
        return Result.Success();
    }

    public async Task<Result> StartShowAsync(string sessionKey)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("StartShow", sessionKey);
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

    public async Task<Result> SendEditModeAction<T>(string sessionKey, EditModeMethod method, T newObject,
        EditParameters? parameters = null)
    {
        if (!TryGetConnectedHub(out HubConnection? hub, out string? error)) return error!;

        return await hub.InvokeAsync<Result>("EditModeAction", sessionKey, method, newObject,
            typeof(T).AssemblyQualifiedName, parameters);
    }

    private bool TryGetConnectedHub([NotNullWhen(true)] out HubConnection? hub, out string? error)
    {
        hub = _showHub;
        if (hub is null)
        {
            error = "ShowHub connection is not established.";
            return false;
        }

        if (hub.State != HubConnectionState.Connected)
        {
            error = "ShowHub connection is not connected.";
            return false;
        }

        error = null;
        return true;
    }
}

public enum Mode
{
    Live,
    Edit,
    Browse
}