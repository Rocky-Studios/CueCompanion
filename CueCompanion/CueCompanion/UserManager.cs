using CueCompanion.Client;

namespace CueCompanion;

public class UserManager
{
    private readonly List<User> Users = [];

    public void AddUser(User user)
    {
        Users.Add(user);
    }

    public User? GetUser(string ipAddress)
    {
        return Users.FirstOrDefault(u => u.IPAddress == ipAddress);
    }
}