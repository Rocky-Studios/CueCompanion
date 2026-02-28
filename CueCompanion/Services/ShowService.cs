using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ShowService : StateSubscriberService
{
    private HubConnection? _showHub;

    private CueRequestResult? LatestCueInfo;
    private ShowRequestResult? LatestShowInfo;
    public Show? CurrentShow => LatestShowInfo?.Show;
    public int? CurrentCuePosition => LatestShowInfo?.CurrentCuePosition;
    public Cue[] CurrentCues => LatestCueInfo?.Cues ?? [];
    public CueTask[] Tasks => LatestCueInfo?.Tasks ?? [];
    public CueTask[] TasksForCurrentCue => Tasks.Where(t => t.CueId == CurrentCue?.Id).ToArray();

    public Cue? CurrentCue => CurrentCues.FirstOrDefault(c => c.Position == CurrentCuePosition);

    public async Task StartAsync(string baseUrl)
    {
        _showHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}api/show")
            .WithAutomaticReconnect()
            .Build();

        _showHub.On("ShowUpdated", (ShowUpdate update) =>
        {
            LatestShowInfo = new ShowRequestResult
            {
                Success = true,
                Show = update.Show,
                CurrentCuePosition = update.CurrentCuePosition
            };
            LatestCueInfo = update.Cues;
            UpdateState();
        });

        await _showHub.StartAsync();
    }

    public async Task<ShowRequestResult> GetCurrentShowAsync(string sessionKey)
    {
        if (_showHub == null)
            throw new InvalidOperationException("ShowHub connection is not established.");

        if (_showHub.State != HubConnectionState.Connected)
            throw new InvalidOperationException("ShowHub connection is not connected.");

        LatestShowInfo = await _showHub.InvokeAsync<ShowRequestResult>("GetCurrentShow", sessionKey);
        UpdateState();
        return LatestShowInfo.Value!;
    }

    public async Task<CueRequestResult> GetCuesForShowAsync(string sessionKey, int showID)
    {
        if (_showHub == null)
            throw new InvalidOperationException("ShowHub connection is not established.");

        if (_showHub.State != HubConnectionState.Connected)
            throw new InvalidOperationException("ShowHub connection is not connected.");

        LatestCueInfo = await _showHub.InvokeAsync<CueRequestResult>("GetCuesForShow", sessionKey, showID);
        UpdateState();
        return LatestCueInfo.Value!;
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
}