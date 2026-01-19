namespace CueCompanion;

public class Show
{
    public DateOnly Date;
    public TimeOnly StartTime;
    public TimeOnly EndTime;
    public string Name;
    public ShowLocation Location;
    public Cue[] Cues;

    public Show(DateOnly date, TimeOnly startTime, TimeOnly endTime, string name, ShowLocation location, Cue[] cues)
    {
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Name = name;
        Location = location;
        Cues = cues;
    }
}

public enum ShowLocation
{
    MPC,
    Piazza
}
