namespace CommanderD.Services;

public class ConsoleService : IConsoleService
{
    public ConsoleColor BackgroundColor => Console.BackgroundColor;

    public ConsoleColor ForegroundColor => Console.ForegroundColor;

    public void SetColors(ConsoleColor background, ConsoleColor foreground)
    {
        Console.BackgroundColor = background;
        Console.ForegroundColor = foreground;
    }

    public void ResetColors()
    {
        Console.ResetColor();
    }

    public void Clear()
    {
        Console.Clear();
    }
}