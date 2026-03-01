using CueCompanion.UserManagement;
using SQLite;

namespace CueCompanion;

public static class PermissionManager
{
    public static void CreateDefaultPermissions()
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        Permission[] permissions =
        [
            new() { Name = "Admin" },
            new() { Name = "ManageUsers" },
            new() { Name = "ViewShow" },
            new() { Name = "ControlShow" },
            new() { Name = "EditShow" }
        ];
        Permission[] existingPermissions = db.Table<Permission>().ToArray();
        Permission[] permissionsToAdd =
            permissions.Where(p => existingPermissions.All(ep => ep.Name != p.Name)).ToArray();
        db.InsertAll(permissionsToAdd);
    }

    public static Permission? GetPermissionByName(string name)
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        return db.Table<Permission>().FirstOrDefault(p => p.Name == name, null);
    }

    public static Permission GetPermissionById(int id)
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        return db.Table<Permission>().FirstOrDefault(p => p.Id == id);
    }

    public static IEnumerable<Permission> GetPermissions()
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        return db.Table<Permission>().ToArray();
    }

    public static bool HasPermission(Permission permission, User user)
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        UserPermission? userPermission = db.Table<UserPermission>()
            .FirstOrDefault(up => up.UserId == user.Id && up.PermissionId == permission.Id, null);
        return userPermission is { Value: true };
    }

    public static bool SetPermission(Permission permission, User user, bool value)
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        UserPermission? userPermission = db.Table<UserPermission>()
            .FirstOrDefault(up => up.UserId == user.Id && up.PermissionId == permission.Id, null);
        if (userPermission == null)
        {
            userPermission = new UserPermission
            {
                UserId = user.Id,
                PermissionId = permission.Id,
                Value = value
            };
            db.Insert(userPermission);
            return true;
        }

        userPermission.Value = value;
        db.Update(userPermission);
        return true;
    }

    public static IEnumerable<Permission> GetPermissionsForUser(User user)
    {
        SQLiteConnection db = DatabaseHandler.Connection;
        IEnumerable<Permission> permissions = from p in db.Table<Permission>()
            join up in db.Table<UserPermission>() on p.Id equals up.PermissionId
            where up.UserId == user.Id && up.Value
            select p;
        return permissions.ToArray();
    }

    public static bool UserHasPermission(string sessionKey, string permission, out string? error)
    {
        User? user = UserManager.GetUserBySessionKey(sessionKey);
        if (user == null) throw new Exception("Invalid session key.");
        bool hasPermission = HasPermission(
            GetPermissionByName(permission) ?? throw new Exception(permission + " permission not found."), user);
        if (!hasPermission)
        {
            error = "Access denied.";
            return false;
        }

        error = null;
        return true;
    }
}