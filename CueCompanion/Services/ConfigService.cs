using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ConfigService : StateSubscriberService
{
    private HubConnection? _configHub;

    public async Task StartAsync()
    {
        _configHub = new HubConnectionBuilder()
                    .WithUrl($"{Program.localhostURL}/api/config")
                    .WithAutomaticReconnect()
                    .Build();

        await _configHub.StartAsync();
        UpdateState();
    }
}