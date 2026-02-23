namespace CueCompanion.Components;

public struct CreateNewUserResult
{
    public string? ErrorMessage { get; init; }

    public CreateNewUserResult(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}