namespace CueCompanion;

public struct ConnectionConfiguration
{
    public string AdminUsername = "admin";
    public int AdminPasswordHash = "password123".GetHashCode();

    public ConnectionConfiguration()
    {
    }
}