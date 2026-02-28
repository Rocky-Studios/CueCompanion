using SQLite;

namespace CueCompanion;

[Table("show_role_assignments")]
public class ShowRoleAssignment
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("show_id")]
    public int ShowID { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }
}