namespace CueCompanion.Client;

public struct ConnectionResult
{
    public required Connection? Connection { get; init; }
    public required string? ErrorMessage { get; init; }
    public required string? SessionKey { get; init; }
}