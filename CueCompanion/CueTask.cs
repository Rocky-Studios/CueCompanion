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
    public string? Tasks { get; set; }
}