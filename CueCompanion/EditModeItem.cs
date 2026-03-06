namespace CueCompanion;

public enum EditModeMethod
{
    Create,
    Update,
    Delete,
    Move
}

public struct EditParameters
{
    // Add any additional parameters needed for the edit action here
    public int Direction { get; set; } // -1 for up, 1 for down (used for Move action)
}