using SQLite;

namespace CueCompanion;

[Table("users")]
public class User
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("name")]
    public string UserName { get; set; } = "Username";

    [Ignore]
    public string Password { get; set; } = "";

    [Column("passwordHash")]
    public string PasswordHash { get; set; } = "";

    [Column("canLogIn")]
    public bool CanLogin { get; set; } = true;

    private Permission[]? _permissions;

    public void SetPermissions(Permission[] permissions)
    {
        _permissions = permissions;
    }

    public bool HasPermission(int permissionID, bool valueIfUnknown = false)
    {
        if (_permissions == null || _permissions.Length == 0) return valueIfUnknown;
        return _permissions.Any(p => p.Id == permissionID);
    }
}