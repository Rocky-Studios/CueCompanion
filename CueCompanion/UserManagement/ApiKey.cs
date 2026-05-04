using SQLite;

namespace CueCompanion;

[Table("api_keys")]
public class ApiKey
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("key")]
    public string Key { get; set; } = "";

    [Column("user_id")]
    public int UserID { get; set; }

    [Column("issued_at")]
    public DateTime IssuedAt { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }
}