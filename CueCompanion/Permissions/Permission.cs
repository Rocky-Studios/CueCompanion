using SQLite;

namespace CueCompanion;

[Table("permissions")]
public class Permission
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("name")] public string Name { get; set; }
}