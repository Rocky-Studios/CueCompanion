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
    public string Name { get; set; } = "New show";

    [Column("start")]
    public DateTime Start { get; set; }

    [Column("end")]
    public DateTime End { get; set; }
}