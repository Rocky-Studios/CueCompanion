namespace CueCompanion;

public struct ShowRequestResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Show? Show { get; set; }
    public int? CurrentCuePosition { get; set; }
    public Role[]? Roles { get; set; }
}