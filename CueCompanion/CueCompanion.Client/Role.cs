namespace CueCompanion.Client;

public class Role
{
    public Role()
    {
    }

    public Role(string roleName, string? personName = null)
    {
        RoleName = roleName;
        PersonName = personName;
    }

    public string? PersonName { get; set; }
    public string RoleName { get; set; } = "";
}