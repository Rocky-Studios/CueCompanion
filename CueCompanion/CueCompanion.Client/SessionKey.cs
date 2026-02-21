using SQLite;

namespace CueCompanion.Client;

[Table("sessionKeys")]
public class SessionKey
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("key")] public string Key { get; set; }

    [Column("connectionId")] public int ConnectionId { get; set; }

    [Column("issuedAt")] public DateTime IssuedAt { get; set; }

    [Column("expiresAt")] public DateTime ExpiresAt { get; set; }
}