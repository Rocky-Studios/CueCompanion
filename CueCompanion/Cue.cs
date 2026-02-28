using SQLite;

namespace CueCompanion;

[Table("cues")]
public class Cue
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("show_id")]
    public int ShowId { get; set; }

    [Column("position")]
    public int Position { get; set; }

    [Column("duration_mins")]
    public int? DurationMins { get; set; }
}