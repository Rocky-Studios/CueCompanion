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
    public CueTask[] CurrentTasks => LatestCueInfo?.Tasks ?? [];

    public async Task StartAsync(string baseUrl)
    {
        _showHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}api/show")
            .WithAutomaticReconnect()
            .Build();

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
}