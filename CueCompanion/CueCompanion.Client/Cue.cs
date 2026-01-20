namespace CueCompanion;

public class Cue
{
    // Parameterless constructor required for System.Text.Json deserialization
    public Cue()
    {
    }

    public Cue(int cueNumber, string cueName, string description, Dictionary<string, string> tasks,
        Dictionary<string, string>? notes = null)
    {
        CueNumber = cueNumber;
        CueName = cueName;
        Description = description;
        Tasks = tasks;
        Notes = notes ?? new Dictionary<string, string>();
    }

    public string CueName { get; set; } = "";
    public int CueNumber { get; set; }
    public string Description { get; set; } = "";
    public Dictionary<string, string> Tasks { get; set; } = new();
    public Dictionary<string, string> Notes { get; set; } = new();
}