using SQLite;

namespace CueCompanion;

public static class ChatManager
{
    private static SQLiteConnection Db => DatabaseHandler.Connection;

    public static Message? AddMessage(string apiKey, string content, out bool success)
    {
        ApiKey api = Db.Table<ApiKey>().FirstOrDefault(s => s.Key == apiKey);
        if (api == null)
        {
            success = false;
            return null;
        }


        User user = Db.Table<User>().FirstOrDefault(u => u.Id == api.UserID);
        if (user == null)
        {
            success = false;
            return null;
        }

        Message message = new()
        {
            Timestamp = DateTime.Now,
            Sender    = user.Id,
            Content   = content,
        };

        Db.Insert(message);
        success = true;
        return message;
    }

    public static Message[] GetAllMessages(string apiKey)
    {
        ApiKey api = Db.Table<ApiKey>().FirstOrDefault(s => s.Key == apiKey);
        if (api == null) return [];

        User user = Db.Table<User>().FirstOrDefault(u => u.Id == api.UserID);
        if (user == null) return [];

        return Db.Table<Message>().ToArray();
    }
}