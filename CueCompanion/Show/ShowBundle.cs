namespace CueCompanion;

public struct ShowBundle
{
    public Show      Show  { get; set; }
    public Cue[]     Cues  { get; set; }
    public CueTask[] Tasks { get; set; }
}