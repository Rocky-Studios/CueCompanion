namespace CueCompanion;

public struct ShowUpdate
{
    public Show?     LiveShow           { get; set; }
    public Show[]    Shows              { get; set; }
    public int?      CurrentCuePosition { get; set; }
    public Cue[]     Cues               { get; set; }
    public CueTask[] Tasks              { get; set; }
    public Role[]    Roles              { get; set; }
}