namespace CommanderD.Commands;


public static class ColorPalette
{
    private static readonly Dictionary<string, ConsoleColor> Colors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "black", ConsoleColor.Black },
            { "white", ConsoleColor.White },
            { "gray", ConsoleColor.Gray },
            { "grey", ConsoleColor.Gray },
            { "darkgray", ConsoleColor.DarkGray },
            { "darkgrey", ConsoleColor.DarkGray },
            { "red", ConsoleColor.Red },
            { "darkred", ConsoleColor.DarkRed },
            { "green", ConsoleColor.Green },
            { "darkgreen", ConsoleColor.DarkGreen },
            { "blue", ConsoleColor.Blue },
            { "darkblue", ConsoleColor.DarkBlue },
            { "yellow", ConsoleColor.Yellow },
            { "darkyellow", ConsoleColor.DarkYellow },
            { "cyan", ConsoleColor.Cyan },
            { "darkcyan", ConsoleColor.DarkCyan },
            { "magenta", ConsoleColor.Magenta },
            { "darkmagenta", ConsoleColor.DarkMagenta },
        };

    public static IReadOnlyList<string> Names { get; } = Colors
        .Where(color => string.Equals(color.Key, color.Value.ToString(), StringComparison.OrdinalIgnoreCase))
        .Select(color => color.Key)
        .ToList();

    public static bool TryGet(string name, out ConsoleColor color)
    {
        return Colors.TryGetValue(name, out color);
    }
}