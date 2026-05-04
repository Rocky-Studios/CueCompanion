using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string apiKey, string content)
    {
        Message? message = ChatManager.AddMessage(apiKey, content, out bool success);
        if (!success || message == null) return;

        await Clients.All.SendAsync("MessageSent", message);
    }

    public Task<Message[]> GetAllMessages(string apiKey)
    {
        try
        {
            var messages = ChatManager.GetAllMessages(apiKey);
            return Task.FromResult(messages);
        }
        catch (Exception exception)
        {
            return Task.FromException<Message[]>(exception);
        }
    }
}