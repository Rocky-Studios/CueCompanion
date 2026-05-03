using CueCompanion.Components;
using CueCompanion.Notes;
using CueCompanion.UserManagement;
using SQLite;

namespace CueCompanion;

public static class DatabaseHandler
{
    public static SQLiteConnection Connection { get; private set; } = null!;

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


        bool hasAdmin = Connection.Table<User>().ToList().Any(c => c.UserName == "admin");
        if (!hasAdmin)
        {
            User adminUser = new()
            {
                UserName     = "admin",
                PasswordHash = Hash.HashPassword("admin")
            };
            Connection.Insert(adminUser);
            Permission? adminPermission       = PermissionManager.GetPermissionByName("Admin");
            Permission? manageUsersPermission = PermissionManager.GetPermissionByName("ManageUsers");
            if (adminPermission != null)
                PermissionManager.SetPermission(adminPermission, adminUser, true);
            if (manageUsersPermission != null)
                PermissionManager.SetPermission(manageUsersPermission, adminUser, true);
        }

        UserManager.RemoveExpiredSessionKeys();
        _purgeTimer = new PeriodicTimer(TimeSpan.FromMinutes(10));
        _ = Task.Run(async () =>
                     {
                         while (true)
                         {
                             PurgeUnreferencedItems();
                             await _purgeTimer.WaitForNextTickAsync();
                         }
                     });
    }

    private static PeriodicTimer _purgeTimer = null!;

    private static void PurgeUnreferencedItems()
    {
        DateTime now = DateTime.Now;
        Console.WriteLine($"Removing unreferenced items from the database... ({now.ToShortTimeString()})");

        //Since deleted a show or a cue doesn't delete all the things associated with it, this is done here
        Connection.Table<Cue>().ToList().ForEach(c =>
                                                 {
                                                     if (Connection.Table<Show>().FirstOrDefault(s => s.Id == c.ShowId) == null)
                                                         Connection.Delete(c);
                                                 });

        Connection.Table<CueTask>().ToList().ForEach(ct =>
                                                     {
                                                         if (Connection.Table<Cue>().FirstOrDefault(c => c.Id == ct.CueId) == null)
                                                             Connection.Delete(ct);
                                                     });

        Connection.Table<Note>().ToList().ForEach(n =>
                                                  {
                                                      if (Connection.Table<Show>().FirstOrDefault(s => s.Id == n.ShowId) == null)
                                                          Connection.Delete(n);
                                                  });
    }
}