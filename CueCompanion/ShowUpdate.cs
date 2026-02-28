namespace CueCompanion;

public struct ShowUpdate
{
    public Show? Show { get; set; }
    public int? CurrentCuePosition { get; set; }
    public CueRequestResult Cues { get; set; }
}