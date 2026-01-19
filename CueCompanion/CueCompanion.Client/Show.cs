using CueCompanion.Client;

namespace CueCompanion;

public class Show
{
    public Cue[] Cues;
    public DateOnly Date;
    public TimeOnly EndTime;
    public ShowLocation Location;
    public string Name;
    public Role[] Roles;
    public TimeOnly StartTime;

    public Show(DateOnly date, TimeOnly startTime, TimeOnly endTime, string name, ShowLocation location, Role[] roles,
        Cue[] cues)
    {
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Name = name;
        Location = location;
        Roles = roles;
        Cues = cues;
    }
}

public enum ShowLocation
{
    MPC,
    Piazza
}