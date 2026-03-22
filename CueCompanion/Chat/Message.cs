using SQLite;

namespace CueCompanion;

[Table("messages")]
public class Message
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Column("user_id")]
    public int Sender { get; set; }

    [Column("content")]
    public string Content { get; set; }
}