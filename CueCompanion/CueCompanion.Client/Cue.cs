namespace CueCompanion;

public class Cue
{
    public int CueNumber;
    public string CueName;
    public string Description;

    public Cue(int cueNumber, string cueName, string description)
    {
        CueNumber = cueNumber;
        CueName = cueName;
        Description = description;
    }
}