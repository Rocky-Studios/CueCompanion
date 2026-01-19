namespace CueCompanion.Client;

public class Role
{
    public string? PersonName;
    public string RoleName;

    public Role(string roleName, string? personName = null)
    {
        RoleName = roleName;
        PersonName = personName;
    }
}