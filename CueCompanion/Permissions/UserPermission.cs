using SQLite;

namespace CueCompanion;

[Table("user_permissions")]
public class UserPermission
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("userId")] public int UserId { get; set; }

    [Column("permissionId")] public int PermissionId { get; set; }

    [Column("value")] public bool Value { get; set; }
}