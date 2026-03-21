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
                    throw new ArgumentException(
                        "Cannot directly modify the live cue position. Use NextCue or PreviousCue instead.");
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
        if (_showHub == null)
            return "ShowHub connection is not established.";

        if (_showHub.State != HubConnectionState.Connected)
            return "ShowHub connection is not connected.";

        Result<(Show?, int?, Role[])> r =
            await _showHub.InvokeAsync<Result<(Show?, int?, Role[])>>("GetCurrentShow", sessionKey);
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
        if (_showHub == null)
            return "ShowHub connection is not established.";

        if (_showHub.State != HubConnectionState.Connected)
            return "ShowHub connection is not connected.";

        Result<(Cue[], CueTask[])> r =
            await _showHub.InvokeAsync<Result<(Cue[], CueTask[])>>("GetCuesForShow", sessionKey, showID);
        if (!r.IsSuccess) return r.Error!;
        (Cue[] cues, CueTask[] tasks) = r.Value;

        Cues = cues;
        Tasks = tasks;

        UpdateState();
        return Result.Success();
    }

    public async Task StartShowAsync(string sessionKey)
    {
        if (_showHub == null)
            throw new InvalidOperationException("ShowHub connection is not established.");

        if (_showHub.State != HubConnectionState.Connected)
            throw new InvalidOperationException("ShowHub connection is not connected.");

        await _showHub.InvokeAsync("StartShow", sessionKey);
    }

    public async Task NextCueAsync(string sessionKey)
    {
        if (_showHub == null)
            throw new InvalidOperationException("ShowHub connection is not established.");

        if (_showHub.State != HubConnectionState.Connected)
            throw new InvalidOperationException("ShowHub connection is not connected.");

        await _showHub.InvokeAsync("NextCue", sessionKey);
    }

    public async Task PreviousCueAsync(string sessionKey)
    {
        if (_showHub == null)
            throw new InvalidOperationException("ShowHub connection is not established.");

        if (_showHub.State != HubConnectionState.Connected)
            throw new InvalidOperationException("ShowHub connection is not connected.");

        await _showHub.InvokeAsync("PreviousCue", sessionKey);
    }

    public async Task<Result> SendEditModeAction<T>(string sessionKey, EditModeMethod method, T newObject,
        EditParameters? parameters = null)
    {
        if (_showHub == null)
            throw new InvalidOperationException("ShowHub connection is not established.");

        if (_showHub.State != HubConnectionState.Connected)
            throw new InvalidOperationException("ShowHub connection is not connected.");

        return await _showHub.InvokeAsync<Result>("EditModeAction", sessionKey, method, newObject,
            typeof(T).AssemblyQualifiedName, parameters);
    }
}

public enum Mode
{
    Live,
    Edit,
    Browse
}