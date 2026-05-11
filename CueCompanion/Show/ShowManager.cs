using SQLite;

namespace CueCompanion;

public static class ShowManager
{
    public static Show? CurrentShow => _currentShowID.HasValue
                                           ? _db.Table<Show>().FirstOrDefault(s => s.Id == _currentShowID.Value)
                                           : null;

    public static  bool IsShowActive { get; set; }
    private static int? _currentShowID;
    public static  int? CurrentCuePosition;

    public static Show? GetShowById(int showID)
    {
        return _db.Table<Show>().FirstOrDefault(s => s.Id == showID);
    }

    public static Role[]    GetRoles() => _db.Table<Role>().ToArray();
    public static Show[]    GetShows() => _db.Table<Show>().ToArray();
    public static Cue[]     GetCues()  => _db.Table<Cue>().ToArray();
    public static CueTask[] GetTasks() => _db.Table<CueTask>().ToArray();

    public static void CreateDefaultRoles()
    {
        Role[] roles =
        [
            new() { Name = "Director" },
            new() { Name = "Stage" },
            new() { Name = "Sound" },
            new() { Name = "Graphics" },
            new() { Name = "Lights" },
            new() { Name = "Camera" },
            new() { Name = "Aux" },
        ];

        var existing = _db.Table<Role>().ToArray();
        Role[] toAdd = roles.Where(newRole => existing.All(existingRole => existingRole.Name != newRole.Name))
                            .ToArray();
        _db.InsertAll(toAdd);
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
        _currentShowID     = null;
        IsShowActive       = false;
        CurrentCuePosition = null;
        return Result.Success();
    }

    public static Result<object> EditAction(string          apiKey, EditModeMethod method, object newObject, Type objectType,
                                            EditParameters? parameters)
    {
        void ReorderCues(int showID)
        {
            var cues       = GetCuesForShow(showID);
            var sortedCues = cues.OrderBy(c => c.Position).ToList();
            if (sortedCues.Count == 0) return;

            // Reorder cues to have sequential positions starting at 1.
            for (int i = 0; i < sortedCues.Count; i++)
            {
                sortedCues[i].Position = i + 1;
                _db.Update(sortedCues[i]);
            }
        }

        int GetIDFromObject(object obj)
        {
            return obj switch
                   {
                       Show s     => s.Id,
                       Cue c      => c.Id,
                       CueTask ct => ct.Id,
                       _          => throw new ArgumentOutOfRangeException(nameof(obj), $"Unsupported object type {obj.GetType()} for getting ID."),
                   };
        }

        AuditActionType EditMethodToAuditMethod()
        {
            return method switch
                   {
                       EditModeMethod.Create    => AuditActionType.Create,
                       EditModeMethod.Update    => AuditActionType.Update,
                       EditModeMethod.Delete    => AuditActionType.Delete,
                       EditModeMethod.DeleteAll => AuditActionType.Delete,
                       EditModeMethod.Move      => AuditActionType.Update,
                       _                        => throw new ArgumentOutOfRangeException(nameof(method), method, null),
                   };
        }

        AuditAction auditAction = new(EditMethodToAuditMethod(), DateTime.UtcNow, apiKey, null, "EDIT MODE ACTION");

        if (!(newObject.GetType() == objectType))
        {
            auditAction.SetErrorAndUpdate($"Type mismatch. Argument {newObject} is not of type {objectType}.");
            return auditAction.Error;
        }

        auditAction.UpdateInDatabase();

        switch (method)
        {
            case EditModeMethod.Create:
            {
                _db.Insert(newObject);
                if (newObject is Cue cue)
                    ReorderCues(cue.ShowId);
                auditAction.Description += $" - Created object of type {objectType} with ID {GetIDFromObject(newObject)}.";
                auditAction.UpdateInDatabase();
                return newObject;
            }
            case EditModeMethod.Update:
            {
                _db.Update(newObject);
                if (newObject is Cue cue)
                    ReorderCues(cue.ShowId);
                auditAction.Description += $" - Updated object of type {objectType} with ID {GetIDFromObject(newObject)}.";
                auditAction.UpdateInDatabase();
                return newObject;
            }
            case EditModeMethod.Delete:
            {
                _db.Delete(newObject);
                if (newObject is Cue cue)
                    ReorderCues(cue.ShowId);
                auditAction.Description += $" - Deleted object of type {objectType} with ID {GetIDFromObject(newObject)}.";
                auditAction.UpdateInDatabase();
                return Result.Success();
            }
            case EditModeMethod.DeleteAll:
            {
                if (newObject is Cue cue)
                {
                    _db.Table<Cue>().ToList()
                       .Where(c => c.ShowId == cue.ShowId)
                       .ToList()
                       .ForEach(c => _db.Delete(c));
                    auditAction.Description += $" - Deleted all cues in show ID{cue.ShowId}";
                    auditAction.UpdateInDatabase();
                    return Result.Success();
                }

                if (newObject is CueTask cueTask)
                {
                    _db.Table<CueTask>().ToList()
                       .Where(ct => ct.CueId == cueTask.CueId)
                       .ToList()
                       .ForEach(ct => _db.Delete(ct));
                    auditAction.Description += $" - Deleted all tasks in cue ID{cueTask.CueId}";
                    auditAction.UpdateInDatabase();
                    return Result.Success();
                }

                auditAction.SetErrorAndUpdate("DeleteAll action is only supported for cues and tasks.");
                return auditAction.Error;
            }
            case EditModeMethod.Move:
            {
                if (newObject is Cue cue && parameters is
                    {
                        Direction: var direction,
                    })
                {
                    Cue? cueBefore = _db.Table<Cue>().ToList()
                                        .FirstOrDefault(c => c.ShowId == cue.ShowId && c.Position == cue.Position - 1);
                    Cue? cueAfter = _db.Table<Cue>().ToList()
                                       .FirstOrDefault(c => c.ShowId == cue.ShowId && c.Position == cue.Position + 1);
                    if ((direction == -1 && cueBefore == null) || (direction == 1 && cueAfter == null))
                    {
                        auditAction.SetErrorAndUpdate("Cannot move cue further in that direction.");
                        return auditAction.Error;
                    }

                    cue.Position += direction;
                    _db.Update(cue);

                    Cue? otherCue = direction == -1 ? cueBefore : cueAfter;
                    if (otherCue == null)
                    {
                        auditAction.SetErrorAndUpdate("Internal server error. Cue to swap with not found.");
                        return auditAction.Error;
                    }

                    // Move the other cue in the opposite direction to swap positions
                    otherCue.Position -= direction;
                    _db.Update(otherCue);
                    auditAction.Description += $" - Moved cue ID {cue.Id} {(direction == -1 ? "up" : "down")} in show ID {cue.ShowId}.";
                    auditAction.UpdateInDatabase();
                    return Result.Success();
                }
                else
                {
                    auditAction.SetErrorAndUpdate("Move action is only implemented for Cues with a Direction parameter.");
                    return auditAction.Error;
                }
            }
            default:
            {
                auditAction.SetErrorAndUpdate($"Not implemented {method} {objectType}");
                return auditAction.Error;
            }
        }
    }

    public static Result<ShowBundle> BundleShow(int showID)
    {
        Show? show = _db.Table<Show>().FirstOrDefault(s => s.Id == showID);
        if (show == null) return Result<ShowBundle>.Failure($"Show with ID {showID} not found.");

        var cues  = GetCuesForShow(showID);
        var tasks = GetTasksForShow(showID);

        return new ShowBundle
        {
            Show  = show,
            Cues  = cues,
            Tasks = tasks,
        };
    }

    public static Result AddShowFromBundle(ShowBundle bundle)
    {
        try
        {
            bundle.Show.Id = 0; // Reset IDs to let the database assign a new one
            _db.Insert(bundle.Show);
            int                  showID   = bundle.Show.Id;
            Dictionary<int, int> cueIdMap = new(); // Map old cue IDs to new ones

            foreach (Cue bundleCue in bundle.Cues)
            {
                int oldId = bundleCue.Id;
                bundleCue.ShowId = showID;
                bundleCue.Id     = 0;
                _db.Insert(bundleCue);
                cueIdMap.Add(oldId, bundleCue.Id);
            }

            foreach (CueTask bundleTask in bundle.Tasks)
            {
                bundleTask.Id = showID;
                bundleTask.Id = 0;
                bundleTask.CueId = cueIdMap.TryGetValue(bundleTask.CueId, out int value)
                                       ? value
                                       : throw new Exception($"Cue ID {bundleTask.CueId} not found. Try re-exporting the show or manually fixing the file.");
                _db.Insert(bundleTask);
            }
        }
        catch (Exception e)
        {
            return Result.Failure("Error adding show from bundle: \n\t" + e.Message);
        }

        return Result.Success();
    }

    public static Result SelectShow(int? showID)
    {
        _currentShowID     = showID;
        CurrentCuePosition = null;
        return Result.Success();
    }
}