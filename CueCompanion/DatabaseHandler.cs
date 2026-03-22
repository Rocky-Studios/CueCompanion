using CueCompanion.Components;
using CueCompanion.Notes;
using CueCompanion.UserManagement;
using SQLite;

namespace CueCompanion;

public static class DatabaseHandler
{
    public static SQLiteConnection Connection { get; private set; }

    public static void Init()
    {
        Connection = new SQLiteConnection("data.db");
        Connection.CreateTable<Show>();
        Connection.CreateTable<User>();
        Connection.CreateTable<SessionKey>();
        Connection.CreateTable<Permission>();
        Connection.CreateTable<UserPermission>();
        Connection.CreateTable<Role>();
        Connection.CreateTable<Cue>();
        Connection.CreateTable<CueTask>();
        Connection.CreateTable<ShowRoleAssignment>();
        Connection.CreateTable<Message>();
        Connection.CreateTable<Note>();


        ShowManager.CreateDefaultRoles();
        PermissionManager.CreateDefaultPermissions();

        // Clear old show state, useful for development
        if (false)
        {
            Connection.DeleteAll<Show>();
            Connection.DeleteAll<Cue>();
            Connection.DeleteAll<CueTask>();
            Connection.DeleteAll<ShowRoleAssignment>();
        }

        // Create a default show
        if (false) ShowManager.CreateDefaultShow();


        bool hasAdmin = Connection.Table<User>().ToList().Any(c => c.UserName == "admin");
        if (!hasAdmin)
        {
            User adminUser = new()
            {
                UserName = "admin",
                PasswordHash = Hash.HashPassword("admin")
            };
            Connection.Insert(adminUser);
            Permission? adminPermission = PermissionManager.GetPermissionByName("Admin");
            Permission? manageUsersPermission = PermissionManager.GetPermissionByName("ManageUsers");
            if (adminPermission != null)
                PermissionManager.SetPermission(adminPermission, adminUser, true);
            if (manageUsersPermission != null)
                PermissionManager.SetPermission(manageUsersPermission, adminUser, true);
        }

        UserManager.RemoveExpiredSessionKeys();
    }
}