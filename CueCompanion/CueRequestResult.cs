namespace CueCompanion;

public struct CueRequestResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Cue[]? Cues { get; set; }
    public CueTask[]? Tasks { get; set; }
}