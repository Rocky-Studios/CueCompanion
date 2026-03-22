using SQLite;

namespace CueCompanion;

[Table("shows")]
public class Show
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("start")]
    public DateTime Start { get; set; }

    [Column("end")]
    public DateTime End { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }
}