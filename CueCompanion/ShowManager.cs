using SQLite;

namespace CueCompanion;

public static class ShowManager
{
    public static Show? CurrentShow;
    public static int? CurrentCuePosition;
    public static bool IsShowActive { get; set; } = false;

    public static Role[] GetRoles()
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        return db.Table<Role>().ToArray();
    }

    public static void CreateDefaultRoles()
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        Role[] roles =
        [
            new() { Name = "Director" },
            new() { Name = "Stage" },
            new() { Name = "Sound" },
            new() { Name = "Graphics" },
            new() { Name = "Lights" },
            new() { Name = "Camera" },
            new() { Name = "Aux" }
        ];

        Role[] existing = db.Table<Role>().ToArray();
        Role[] toAdd = roles.Where(newRole => existing.All(existingRole => existingRole.Name != newRole.Name))
            .ToArray();
        db.InsertAll(toAdd);
    }

    public static void CreateDefaultShow()
    {
        SQLiteConnection db = DatabaseHandler.Connection;

        // Create the show
        Show show = new()
        {
            Name = "Music Night Showcase",
            Description = "Auto‑generated from cue sheet.",
            Notes = "Imported from image + example data.",
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(3)
        };
        db.Insert(show);

        // Create cues based on the image
        Cue cue1 = new() { ShowId = show.Id, Name = "Big Band", Position = 1 };
        Cue cue2 = new() { ShowId = show.Id, Name = "Rock Band", Position = 2 };
        Cue cue3 = new() { ShowId = show.Id, Name = "Choir", Position = 3, DurationMins = 4 };
        Cue cue4 = new() { ShowId = show.Id, Name = "Concert Band", Position = 4 };
        Cue cue5 = new() { ShowId = show.Id, Name = "Big Band (Reprise)", Position = 5 };

        db.Insert(cue1);
        db.Insert(cue2);
        db.Insert(cue3);
        db.Insert(cue4);
        db.Insert(cue5);

        // Lookup roles
        Role soundRole = db.Table<Role>().First(r => r.Name == "Sound");
        Role cameraRole = db.Table<Role>().First(r => r.Name == "Camera");
        Role stageRole = db.Table<Role>().First(r => r.Name == "Stage");

        // Assign roles to user 1 for this show
        db.Insert(new ShowRoleAssignment { ShowID = show.Id, RoleId = soundRole.Id, UserId = 1 });
        db.Insert(new ShowRoleAssignment { ShowID = show.Id, RoleId = cameraRole.Id, UserId = 1 });
        db.Insert(new ShowRoleAssignment { ShowID = show.Id, RoleId = stageRole.Id, UserId = 1 });

        // --- Cue Tasks from the image ---

        // CUE 1 — Big Band
        db.Insert(new CueTask
        {
            CueId = cue1.Id,
            RoleId = soundRole.Id,
            Tasks = "Mute M5 1-6; Mute Stage Overheads; Unmute Piano, Trumpets, Trombone, Sax, Guitar Amp, Bass Amp"
        });
        db.Insert(new CueTask
        {
            CueId = cue1.Id,
            RoleId = cameraRole.Id,
            Tasks = "Camera zoom out and pan across MPC"
        });
        db.Insert(new CueTask
        {
            CueId = cue1.Id,
            RoleId = stageRole.Id,
            Tasks = "Curtains closed"
        });

        // CUE 2 — Rock Band
        db.Insert(new CueTask
        {
            CueId = cue2.Id,
            RoleId = soundRole.Id,
            Tasks = "Maintain band mix; prepare for guitar solo"
        });
        db.Insert(new CueTask
        {
            CueId = cue2.Id,
            RoleId = cameraRole.Id,
            Tasks = "Watch for sax solo"
        });
        db.Insert(new CueTask
        {
            CueId = cue2.Id,
            RoleId = stageRole.Id,
            Tasks = "No stage movement"
        });

        // CUE 3 — Choir (highlighted in red)
        db.Insert(new CueTask
        {
            CueId = cue3.Id,
            RoleId = soundRole.Id,
            Tasks = "Bring up choir mics; reduce band levels"
        });
        db.Insert(new CueTask
        {
            CueId = cue3.Id,
            RoleId = cameraRole.Id,
            Tasks = "When conductor raises hands, zoom in on choir"
        });
        db.Insert(new CueTask
        {
            CueId = cue3.Id,
            RoleId = stageRole.Id,
            Tasks = "Curtains open slowly"
        });

        // CUE 4 — Concert Band
        db.Insert(new CueTask
        {
            CueId = cue4.Id,
            RoleId = soundRole.Id,
            Tasks = "Balance woodwinds; reduce brass"
        });
        db.Insert(new CueTask
        {
            CueId = cue4.Id,
            RoleId = cameraRole.Id,
            Tasks = "Wide shot of full ensemble"
        });
        db.Insert(new CueTask
        {
            CueId = cue4.Id,
            RoleId = stageRole.Id,
            Tasks = "Music stands repositioned"
        });

        // CUE 5 — Big Band (Reprise)
        db.Insert(new CueTask
        {
            CueId = cue5.Id,
            RoleId = soundRole.Id,
            Tasks = "Return to Big Band mix"
        });
        db.Insert(new CueTask
        {
            CueId = cue5.Id,
            RoleId = cameraRole.Id,
            Tasks = "Track trumpet section"
        });
        db.Insert(new CueTask
        {
            CueId = cue5.Id,
            RoleId = stageRole.Id,
            Tasks = "Curtains half‑closed for finale"
        });
    }

    public static void Init()
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        CurrentShow = db.Table<Show>().FirstOrDefault();
    }

    public static Cue[] GetCuesForShow(int showID)
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        return db.Table<Cue>().Where(c => c.ShowId == showID).ToArray();
    }

    public static CueTask[] GetTasksForShow(int showID)
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        Cue[] cues = GetCuesForShow(showID);
        List<int> cueIds = cues.Select(c => c.Id).ToList();
        return db.Table<CueTask>().Where(ct => cueIds.Contains(ct.CueId)).ToArray();
    }

    public static void StartShow()
    {
        if (CurrentShow == null)
            throw new InvalidOperationException("No show is currently loaded.");
        Cue[] cues = GetCuesForShow(CurrentShow.Id);

        IsShowActive = true;
        CurrentCuePosition = cues.OrderBy(c => c.Position).FirstOrDefault()?.Position ?? 1;
    }
}