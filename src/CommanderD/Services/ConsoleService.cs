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
                // Kein echtes Konsolenfenster (z.B. umgeleitete Ausgabe)
                return DefaultWindowHeight;
            }
        }
    }

    public ConsoleKeyInfo ReadKey()
    {
        if (Console.IsInputRedirected)
        {
            // Ohne Tastatur einfach weiterblättern
            return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
        }

        // Ctrl+C als Taste lesen, damit es nur MORE beendet und nicht die ganze Shell
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

    private const int DefaultWindowHeight = 25;
}