using SQLite;

namespace CueCompanion;

[Table("user_permissions")]
public class UserPermission
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("permission_id")]
    public int PermissionId { get; set; }

    [Column("value")]
    public bool Value { get; set; }
}