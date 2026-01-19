namespace CueCompanion.Client;

public class Connection
{
    public string Role;
    public List<(string view, bool isViewing)> Viewing = [];

    public Connection(string role)
    {
        Role = role;
    }
}