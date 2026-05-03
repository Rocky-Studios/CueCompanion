using CueCompanion.Notes;

namespace CueCompanion;

public struct ShowBundle
{
    public Show      Show  { get; set; }
    public Cue[]     Cues  { get; set; }
    public Note[]    Notes { get; set; }
    public CueTask[] Tasks { get; set; }
}