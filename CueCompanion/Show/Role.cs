using SQLite;

namespace CueCompanion;

[Table("roles")]
public class Role
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }
}