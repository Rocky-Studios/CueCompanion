namespace CueCompanion.Client;

public class User
{
    public User(UserType userType, string ipAddress)
    {
        UserType = userType;
        IPAddress = ipAddress;
        UserId = null;
    }

    public User()
    {
    }

    public UserType UserType { get; set; }
    public string IPAddress { get; set; }
    public Guid? UserId { get; set; }
    public Dictionary<UserPermission, bool>? _permissions { get; set; }

    public Dictionary<UserPermission, bool> GetPermissions()
    {
        return _permissions ?? new Dictionary<UserPermission, bool>();
    }

    public bool HasPermission(UserPermission permission)
    {
        if (_permissions == null) return false;
        if (_permissions.TryGetValue(permission, out bool hasPermission)) return hasPermission;

        return false;
    }

    public void SetPermission(UserPermission permission, bool value)
    {
        if (_permissions == null) _permissions = new Dictionary<UserPermission, bool>();

        _permissions[permission] = value;
    }

    public void SetAllPermissions(bool value)
    {
        if (_permissions == null) _permissions = new Dictionary<UserPermission, bool>();

        foreach (UserPermission permission in Enum.GetValues<UserPermission>()) _permissions[permission] = value;
    }

    public User WithAllPermissions()
    {
        SetAllPermissions(true);
        return this;
    }
}

public enum UserType
{
    Master,
    Child
}

public enum UserPermission
{
    View,
    Explore,
    ChangeCueNumber,
    ModifyNotes,
    ModifyShow
}