namespace CueCompanion.Client;

public class UserConnectPacket
{
    public Connection? Connection { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}