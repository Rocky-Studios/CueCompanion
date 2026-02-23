namespace CueCompanion;

public struct UserConnectionResult
{
    public required User? User { get; init; }
    public required string? ErrorMessage { get; init; }
    public required string? SessionKey { get; init; }
}