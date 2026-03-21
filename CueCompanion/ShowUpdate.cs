namespace CueCompanion;

public struct ShowUpdate
{
    public Show? Show { get; set; }
    public int? CurrentCuePosition { get; set; }
    public Cue[] Cues { get; set; }
    public CueTask[] Tasks { get; set; }
    public Role[] Roles { get; set; }
}