using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ConfigService : StateSubscriberService
{
    private HubConnection? _configHub;

    public List<ApiKey> ApiKeys = [];

    public async Task StartAsync(string baseUrl)
    {
        _configHub = new HubConnectionBuilder()
                    .WithUrl($"{baseUrl}api/config")
                    .WithAutomaticReconnect()
                    .Build();

        await _configHub.StartAsync();
        UpdateState();
    }

    public async Task<Result<ApiKey[]>> GetApiKeys(string apikey)
    {
        var r = await _configHub.InvokeAsync<Result<ApiKey[]>>("GetApiKeys", apikey);
        if (!r.IsSuccess) return r;
        ApiKeys = r.GetValue().ToList();
        return r;
    }
}