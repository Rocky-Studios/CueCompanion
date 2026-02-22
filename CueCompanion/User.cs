using SQLite;

namespace CueCompanion;

[Table("users")]
public class User
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("name")] public string UserName { get; set; }

    [Ignore] public string Password { get; set; }

    [Column("passwordHash")] public string PasswordHash { get; set; }
}