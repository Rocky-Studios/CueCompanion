namespace CueCompanion.Client;

public class ConnectionsPacket
{
    public Connection[]? Connections { get; set; }
    public string? Error { get; set; }
}