using Microsoft.AspNetCore.SignalR;

namespace CueCompanion.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string sessionKey, string content)
    {
        Message? message = ChatManager.AddMessage(sessionKey, content, out bool success);
        if (!success || message == null) return;

        await Clients.All.SendAsync("MessageSent", message);
    }

    public async Task<Message[]> GetAllMessages(string sessionKey)
    {
        Message[] messages = ChatManager.GetAllMessages(sessionKey);
        return messages;
    }
}