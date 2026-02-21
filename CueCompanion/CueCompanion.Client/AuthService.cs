namespace CueCompanion.Client;

public class AuthService
{
    public Connection? Connection { get; private set; }

    public event Action? OnStateChanged;

    public void SetConnection(Connection connection)
    {
        Connection = connection;
        OnStateChanged?.Invoke();
    }
}