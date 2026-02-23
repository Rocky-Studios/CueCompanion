namespace CueCompanion.UserManagement;

public struct UserInfo
{
    public int UserID {get; set;}
    public string UserName {get; set;}
    public Permission[] Permissions {get; set;}
}