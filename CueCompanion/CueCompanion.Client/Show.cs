using CueCompanion.Client;

namespace CueCompanion;

public class Show
{
    public DateOnly Date;
    public TimeOnly StartTime;
    public TimeOnly EndTime;
    public string Name;
    public ShowLocation Location;
    public Role[] Roles;
    public Cue[] Cues;

    public Show(DateOnly date, TimeOnly startTime, TimeOnly endTime, string name, ShowLocation location, Role[] roles, Cue[] cues)
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
