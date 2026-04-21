using SQLite;

namespace CueCompanion;

public static class ShowManager
{
    public static Show? CurrentShow => CurrentShowID.HasValue
                                           ? _db.Table<Show>().FirstOrDefault(s => s.Id == CurrentShowID.Value)
                                           : null;

    public static  bool IsShowActive { get; set; }
    private static int? CurrentShowID;
    public static  int? CurrentCuePosition;

    public static Show? GetShowById(int showID)
    {
        return _db.Table<Show>().FirstOrDefault(s => s.Id == showID);
    }

    public static Role[] GetRoles() => _db.Table<Role>().ToArray();

    public static void CreateDefaultRoles()
    {
        SQLiteConnection _db = DatabaseHandler.Connection;
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

        var existing = _db.Table<Role>().ToArray();
        Role[] toAdd = roles.Where(newRole => existing.All(existingRole => existingRole.Name != newRole.Name))
                            .ToArray();
        _db.InsertAll(toAdd);
    }

    public static void CreateDefaultShow()
    {
        SQLiteConnection db = DatabaseHandler.Connection;

        // Create the show
        Show show = new()
        {
            Name        = "Music Night Showcase",
            Description = "Auto‑generated from cue sheet.",
            Notes       = "Imported from image + example data.",
            Start       = DateTime.Now,
            End         = DateTime.Now.AddHours(3),
        };
        db.Insert(show);

        // Create cues based on the image
        Cue cue1 = new() { ShowId = show.Id, Name = "Big Band", Position           = 1 };
        Cue cue2 = new() { ShowId = show.Id, Name = "Rock Band", Position          = 2 };
        Cue cue3 = new() { ShowId = show.Id, Name = "Choir", Position              = 3, DurationMins = 4 };
        Cue cue4 = new() { ShowId = show.Id, Name = "Concert Band", Position       = 4 };
        Cue cue5 = new() { ShowId = show.Id, Name = "Big Band (Reprise)", Position = 5 };

        db.Insert(cue1);
        db.Insert(cue2);
        db.Insert(cue3);
        db.Insert(cue4);
        db.Insert(cue5);

        // Lookup roles
        Role soundRole  = db.Table<Role>().First(r => r.Name == "Sound");
        Role cameraRole = db.Table<Role>().First(r => r.Name == "Camera");
        Role stageRole  = db.Table<Role>().First(r => r.Name == "Stage");

        // Assign roles to user 1 for this show
        db.Insert(new ShowRoleAssignment { ShowID = show.Id, RoleId = soundRole.Id, UserId  = 1 });
        db.Insert(new ShowRoleAssignment { ShowID = show.Id, RoleId = cameraRole.Id, UserId = 1 });
        db.Insert(new ShowRoleAssignment { ShowID = show.Id, RoleId = stageRole.Id, UserId  = 1 });

        // --- Cue Tasks from the image ---

        // CUE 1 — Big Band
        db.Insert(new CueTask
        {
            CueId  = cue1.Id,
            RoleId = soundRole.Id,
            Tasks  = "Mute M5 1-6; Mute Stage Overheads; Unmute Piano, Trumpets, Trombone, Sax, Guitar Amp, Bass Amp",
        });
        db.Insert(new CueTask
        {
            CueId  = cue1.Id,
            RoleId = cameraRole.Id,
            Tasks  = "Camera zoom out and pan across MPC",
        });
        db.Insert(new CueTask
        {
            CueId  = cue1.Id,
            RoleId = stageRole.Id,
            Tasks  = "Curtains closed",
        });

        // CUE 2 — Rock Band
        db.Insert(new CueTask
        {
            CueId  = cue2.Id,
            RoleId = soundRole.Id,
            Tasks  = "Maintain band mix; prepare for guitar solo",
        });
        db.Insert(new CueTask
        {
            CueId  = cue2.Id,
            RoleId = cameraRole.Id,
            Tasks  = "Watch for sax solo",
        });
        db.Insert(new CueTask
        {
            CueId  = cue2.Id,
            RoleId = stageRole.Id,
            Tasks  = "No stage movement",
        });

        // CUE 3 — Choir (highlighted in red)
        db.Insert(new CueTask
        {
            CueId  = cue3.Id,
            RoleId = soundRole.Id,
            Tasks  = "Bring up choir mics; reduce band levels",
        });
        db.Insert(new CueTask
        {
            CueId  = cue3.Id,
            RoleId = cameraRole.Id,
            Tasks  = "When conductor raises hands, zoom in on choir",
        });
        db.Insert(new CueTask
        {
            CueId  = cue3.Id,
            RoleId = stageRole.Id,
            Tasks  = "Curtains open slowly",
        });

        // CUE 4 — Concert Band
        db.Insert(new CueTask
        {
            CueId  = cue4.Id,
            RoleId = soundRole.Id,
            Tasks  = "Balance woodwinds; reduce brass",
        });
        db.Insert(new CueTask
        {
            CueId  = cue4.Id,
            RoleId = cameraRole.Id,
            Tasks  = "Wide shot of full ensemble",
        });
        db.Insert(new CueTask
        {
            CueId  = cue4.Id,
            RoleId = stageRole.Id,
            Tasks  = "Music stands repositioned",
        });

        // CUE 5 — Big Band (Reprise)
        db.Insert(new CueTask
        {
            CueId  = cue5.Id,
            RoleId = soundRole.Id,
            Tasks  = "Return to Big Band mix",
        });
        db.Insert(new CueTask
        {
            CueId  = cue5.Id,
            RoleId = cameraRole.Id,
            Tasks  = "Track trumpet section",
        });
        db.Insert(new CueTask
        {
            CueId  = cue5.Id,
            RoleId = stageRole.Id,
            Tasks  = "Curtains half‑closed for finale",
        });
    }

    private static SQLiteConnection _db = DatabaseHandler.Connection;

    public static void Init()
    {
    }

    public static Cue[] GetCuesForShow(int showID)
    {
        _db = DatabaseHandler.Connection;
        return _db.Table<Cue>().Where(c => c.ShowId == showID).ToArray();
    }

    public static CueTask[] GetTasksForShow(int showID)
    {
        _db = DatabaseHandler.Connection;
        var       cues   = GetCuesForShow(showID);
        List<int> cueIds = cues.Select(c => c.Id).ToList();
        return _db.Table<CueTask>().Where(ct => cueIds.Contains(ct.CueId)).ToArray();
    }

    public static Result StartShow()
    {
        if (CurrentShow == null) return "Cannot start show. No show loaded.";
        Cue[] cues = GetCuesForShow(CurrentShow.Id);

        IsShowActive       = true;
        CurrentCuePosition = cues.OrderBy(c => c.Position).FirstOrDefault()?.Position ?? 1;
        return Result.Success();
    }


    public static Result StopShow()
    {
        if (CurrentShow == null) return "Cannot stop show. No show loaded.";
        CurrentShowID      = null;
        IsShowActive       = false;
        CurrentCuePosition = null;
        return Result.Success();
    }

    public static Result<object> EditAction(EditModeMethod  method, object newObject, Type objectType,
                                            EditParameters? parameters)
    {
        if (!(newObject.GetType() == objectType))
        {
            return "Type mismatch. Argument " + newObject + " is not of type" + objectType;
        }

        switch (method)
        {
            case EditModeMethod.Create:
            {
                _db.Insert(newObject);
                return newObject;
            }
            case EditModeMethod.Update:
            {
                _db.Update(newObject);
                return newObject;
            }
            case EditModeMethod.Delete:
            {
                _db.Delete(newObject);
                return Result.Success();
            }
            case EditModeMethod.Move:
            {
                if (newObject is Cue cue && parameters is
                    {
                        Direction: var direction
                    })
                {
                    Cue? cueBefore = _db.Table<Cue>().ToList()
                                        .FirstOrDefault(c => c.ShowId == cue.ShowId && c.Position == cue.Position - 1);
                    Cue? cueAfter = _db.Table<Cue>().ToList()
                                       .FirstOrDefault(c => c.ShowId == cue.ShowId && c.Position == cue.Position + 1);
                    if (direction == -1 && cueBefore == null || direction == 1 && cueAfter == null)
                    {
                        return "Cannot move cue further in that direction.";
                    }

                    cue.Position += direction;
                    _db.Update(cue);

                    Cue? otherCue = direction == -1 ? cueBefore : cueAfter;
                    if (otherCue == null) return "Internal server error";

                    // Move the other cue in the opposite direction to swap positions
                    otherCue.Position -= direction;
                    _db.Update(otherCue);
                    return Result.Success();
                }
                else
                {
                    return "Move action is only implemented for Cues with a Direction parameter.";
                }
            }
            default: return "Not implemented " + method + " " + objectType;
        }
    }

    public static Result SelectShow(int? showID)
    {
        CurrentShowID      = showID;
        CurrentCuePosition = null;
        return Result.Success();
    }
}