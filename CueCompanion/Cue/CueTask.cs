using MudBlazor;
using SQLite;

namespace CueCompanion;

[Table("cue_tasks")]
public class CueTask
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("cue_id")]
    public int CueId { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; } // Used to assign a task to a specific role, or null to assign to all roles

    [Column("tasks")]
    public string Tasks { get; set; } = "New task";

    [Column("icon")]
    public CueTaskIcon Icon { get; set; } = CueTaskIcon.Task;

    public static string CueTaskIconToString(CueTaskIcon icon)
    {
        return icon switch
               {
                   CueTaskIcon.Task    => Icons.Material.Filled.Task,
                   CueTaskIcon.Warning => Icons.Material.Filled.Warning,
                   CueTaskIcon.Note    => Icons.Material.Filled.StickyNote2,
                   _                   => Icons.Material.Filled.Task,
               };
    }
}

public enum CueTaskIcon
{
    Task    = 0,
    Note    = 1,
    Warning = 2,
}