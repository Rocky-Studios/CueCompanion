using SQLite;

namespace CueCompanion;

[Table("shows")]
public class Show
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("name")] public required string Name { get; set; }
}