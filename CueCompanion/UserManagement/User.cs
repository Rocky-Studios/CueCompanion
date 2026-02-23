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
    
    private Permission[]? _permissions;
    public void SetPermissions(Permission[] permissions)
    {
        _permissions = permissions;
    }

    public bool HasPermission(int permissionID)
    {
        if(_permissions == null || _permissions.Length == 0) return false;
        return _permissions.Any(p => p.Id == permissionID);
    }
}