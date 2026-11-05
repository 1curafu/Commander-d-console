namespace CommanderD.Services;

public class ConsoleService : IConsoleService
{
    private const int DefaultWindowHeight = 25;

    public ConsoleColor BackgroundColor => Console.BackgroundColor;

    public ConsoleColor ForegroundColor => Console.ForegroundColor;

    public int WindowHeight
    {
        get
        {
            try
            {
                return Console.WindowHeight > 0 ? Console.WindowHeight : DefaultWindowHeight;
            }
            catch (IOException)
            {
                // No real console window (e.g. output is redirected).
                return DefaultWindowHeight;
            }
        }
    }

    public void SetColors(ConsoleColor background, ConsoleColor foreground)
    {
        Console.BackgroundColor = background;
        Console.ForegroundColor = foreground;

        Console.Clear();
    }

    public void ResetColors()
    {
        Console.ResetColor();
        Console.Clear();
    }

    public void Clear()
    {
        Console.Clear();
    }

    public ConsoleKeyInfo ReadKey()
    {
        if (Console.IsInputRedirected)
        {
            // No keyboard available: behave as if Space was pressed so the whole file is shown.
            return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
        }

        bool previous = Console.TreatControlCAsInput;
        Console.TreatControlCAsInput = true;

        try
        {
            return Console.ReadKey(intercept: true);
        }
        finally
        {
            Console.TreatControlCAsInput = previous;
        }
    }
}
