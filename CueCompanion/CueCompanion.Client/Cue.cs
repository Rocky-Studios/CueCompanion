using CueCompanion.Client;

namespace CueCompanion;

public class Cue
{
    public string CueName;
    public int CueNumber;
    public string Description;
    public Dictionary<Role, string> Tasks;

    public Cue(int cueNumber, string cueName, string description, Dictionary<Role, string> tasks)
    {
        CueNumber = cueNumber;
        CueName = cueName;
        Description = description;
        Tasks = tasks;
    }
}