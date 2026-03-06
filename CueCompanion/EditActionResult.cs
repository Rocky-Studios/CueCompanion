namespace CueCompanion;

public struct EditActionResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    
    public static implicit operator EditActionResult (string error)
    {
        return new EditActionResult
        {
            Success = false,
            Error = error
        };
    }

    public static implicit operator EditActionResult(bool success)
    {
        return new EditActionResult
        {
            Success = success,
            Error = null
        };
    }
}