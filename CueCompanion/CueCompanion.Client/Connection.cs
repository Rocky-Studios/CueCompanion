using SQLite;

namespace CueCompanion.Client;

[Table("connections")]
public class Connection
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("name")] public string ConnectionName { get; set; }

    [Ignore] public string Password { get; set; }

    [Column("passwordHash")] public string PasswordHash { get; set; }
}