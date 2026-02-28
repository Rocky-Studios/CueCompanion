using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ShowService : StateSubscriberService
{
    private HubConnection? _showHub;
    public ShowRequestResult? LatestInfo;

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

        LatestInfo = await _showHub.InvokeAsync<ShowRequestResult>("GetCurrentShow", sessionKey);
        UpdateState();
        return LatestInfo.Value!;
    }
}