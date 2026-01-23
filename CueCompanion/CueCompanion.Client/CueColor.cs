namespace CueCompanion.Client;

public enum CueColor
{
    Red,
    Green,
    Blue,
    Yellow,
    Gray
}

public static class CueColorExtensions
{
    public static string ToString(this CueColor color)
    {
        return color switch
        {
            CueColor.Red => "red",
            CueColor.Green => "green",
            CueColor.Blue => "blue",
            CueColor.Yellow => "yellow",
            CueColor.Gray => "gray",
            _ => "gray"
        };
    }
}