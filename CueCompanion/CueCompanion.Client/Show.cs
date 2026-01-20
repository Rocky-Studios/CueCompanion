using CueCompanion.Client;

namespace CueCompanion;

public class Show
{
    // Parameterless constructor required for System.Text.Json deserialization
    public Show()
    {
    }

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

    public Cue[] Cues { get; set; } = Array.Empty<Cue>();
    public DateOnly Date { get; set; }
    public TimeOnly EndTime { get; set; }
    public ShowLocation Location { get; set; }
    public string Name { get; set; } = "";
    public Role[] Roles { get; set; } = Array.Empty<Role>();
    public TimeOnly StartTime { get; set; }

    public static Show CreateDefault()
    {
        return new Show
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = new TimeOnly(19, 0),
            EndTime = new TimeOnly(21, 0),
            Name = "New Show",
            Location = ShowLocation.MPC,
            Roles = Array.Empty<Role>(),
            Cues = Array.Empty<Cue>()
        };
    }
}

public enum ShowLocation
{
    MPC,
    Piazza
}