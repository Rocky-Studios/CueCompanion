using SQLite;

namespace CueCompanion;

public static class ChatManager
{
    private static SQLiteConnection Db => DatabaseHandler.Connection;

    public static Message? AddMessage(string sessionKey, string content, out bool success)
    {
        SessionKey session = Db.Table<SessionKey>().FirstOrDefault(s => s.Key == sessionKey);
        if (session == null)
        {
            success = false;
            return null;
        }


        User user = Db.Table<User>().FirstOrDefault(u => u.Id == session.ConnectionId);
        if (user == null)
        {
            success = false;
            return null;
        }

        Message message = new()
        {
            Timestamp = DateTime.Now,
            Sender = user.Id,
            Content = content
        };

        Db.Insert(message);
        success = true;
        return message;
    }

    public static Message[] GetAllMessages(string sessionKey)
    {
        SessionKey session = Db.Table<SessionKey>().FirstOrDefault(s => s.Key == sessionKey);
        if (session == null) return [];

        User user = Db.Table<User>().FirstOrDefault(u => u.Id == session.ConnectionId);
        if (user == null) return [];

        return Db.Table<Message>().ToArray();
    }
}