using SQLite;

namespace CueCompanion;

[Table("audit_actions")]
public class AuditAction
{
    public AuditAction()
    {
    }

    public AuditAction(AuditActionType type, DateTime timestamp, string apiKey, int? itemID, string description)
    {
        Type        = type;
        Timestamp   = timestamp;
        APIKey      = apiKey;
        ItemID      = itemID;
        Description = description;
        DatabaseHandler.Connection.Insert(this);
    }

    public AuditAction(AuditActionType type, DateTime timestamp, string apiKey, int? itemID, string description, bool success, string error)
    {
        Type        = type;
        Timestamp   = timestamp;
        APIKey      = apiKey;
        ItemID      = itemID;
        Description = description;
        Success     = success;
        Error       = error;
        DatabaseHandler.Connection.Insert(this);
    }

    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("type")]
    public AuditActionType Type { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Column("api_key")]
    public string APIKey { get; set; } = string.Empty;

    [Column("item_id")]
    public int? ItemID { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("success")]
    public bool Success { get; set; } = true;

    [Column("error")]
    public string Error { get; set; } = string.Empty;

    public void SetErrorAndUpdate(string error)
    {
        Error = error;
        UpdateInDatabase();
    }

    public void UpdateInDatabase()
    {
        DatabaseHandler.Connection.Update(this);
    }
}

public enum AuditActionType
{
    Create = 0,
    Update = 1,
    Delete = 2,
    Other  = 3,
}