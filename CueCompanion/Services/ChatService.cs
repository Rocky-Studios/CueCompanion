using Microsoft.AspNetCore.SignalR.Client;

namespace CueCompanion.Services;

public class ChatService : StateSubscriberService, IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        Messages.Clear();
        if (_chatHub != null) await _chatHub.DisposeAsync();
    }

    public readonly List<Message>           Messages = [];
    private         HubConnection?          _chatHub;
    public          Dictionary<int, string> UserIdToNameCache = new();

    public async Task StartAsync()
    {
        _chatHub = new HubConnectionBuilder()
                  .WithUrl($"{Program.localhostURL}/api/chat")
                  .WithAutomaticReconnect()
                  .Build();

        _chatHub.On("MessageSent", (Message message) =>
                                   {
                                       Messages.Add(message);
                                       UpdateState();
                                   });

        await _chatHub.StartAsync();
        UpdateState();
    }

    public async Task SendMessage(string sessionKey, string content)
    {
        if (_chatHub == null) return;
        await _chatHub.InvokeAsync("SendMessage", sessionKey, content);
    }

    public async Task GetAllMessages(string sessionKey)
    {
        if (_chatHub == null) return;
        Message[] getMessages = await _chatHub.InvokeAsync<Message[]>("GetAllMessages", sessionKey);
        Messages.Clear();
        Messages.AddRange(getMessages);
        UpdateState();
    }
}