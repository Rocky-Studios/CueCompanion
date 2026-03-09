using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ConfigService : StateSubscriberService
{
    private HubConnection? _configHub;

    public async Task StartAsync(string baseUrl)
    {
        _configHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}api/config")
            .WithAutomaticReconnect()
            .Build();

        await _configHub.StartAsync();
        UpdateState();
    }
}