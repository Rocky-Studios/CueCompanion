using SQLite;

namespace CueCompanion;

public static class ShowManager
{
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

        Show show = new()
        {
            Name = "Default Show",
            Description = "Example description.",
            Notes = "Example notes.",
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(2)
        };
        db.Insert(show);

        Cue cue1 = new()
        {
            ShowId = show.Id,
            Name = "Cue 1",
            Position = 1
        };
        Cue cue2 = new()
        {
            ShowId = show.Id,
            Name = "Cue 2",
            Position = 2
        };
        Cue cue3 = new()
        {
            ShowId = show.Id,
            Name = "Cue 3",
            Position = 3,
            DurationMins = 4
        };
        db.Insert(cue1);
        db.Insert(cue2);
        db.Insert(cue3);

        Role soundRole = db.Table<Role>().FirstOrDefault(r => r.Name == "Sound");
        Role lightsRole = db.Table<Role>().FirstOrDefault(r => r.Name == "Lights");
        Role graphicsRole = db.Table<Role>().FirstOrDefault(r => r.Name == "Graphics");

        ShowRoleAssignment soundAssignment = new()
        {
            ShowID = show.Id,
            RoleId = soundRole.Id,
            UserId = 1
        };
        ShowRoleAssignment lightsAssignment = new()
        {
            ShowID = show.Id,
            RoleId = lightsRole.Id,
            UserId = 1
        };
        ShowRoleAssignment graphicsAssignment = new()
        {
            ShowID = show.Id,
            RoleId = graphicsRole.Id,
            UserId = 1
        };
        db.Insert(soundAssignment);
        db.Insert(lightsAssignment);
        db.Insert(graphicsAssignment);

        CueTask task1 = new()
        {
            CueId = cue1.Id,
            RoleId = soundRole.Id,
            Tasks = "Prepare audio levels"
        };
        CueTask task2 = new()
        {
            CueId = cue2.Id,
            RoleId = lightsRole.Id,
            Tasks = "Fade to blue lighting"
        };
        CueTask task3 = new()
        {
            CueId = cue3.Id,
            RoleId = graphicsRole.Id,
            Tasks = "Display title slide"
        };
        db.Insert(task1);
        db.Insert(task2);
        db.Insert(task3);
    }
}