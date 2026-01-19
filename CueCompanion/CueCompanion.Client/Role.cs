namespace CueCompanion.Client;

public class Role
{
    public string RoleName;
    public string? PersonName;

    public Role(string roleName, string? personName = null)
    {
        RoleName = roleName;
        PersonName = personName;
    }
}