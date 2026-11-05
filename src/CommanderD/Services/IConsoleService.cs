namespace CommanderD.Services;

public interface IConsoleService
{
    ConsoleColor BackgroundColor { get; }

    ConsoleColor ForegroundColor { get; }

    /// <summary>Number of text rows visible in the terminal window.</summary>
    int WindowHeight { get; }

    void SetColors(ConsoleColor background, ConsoleColor foreground);

    void ResetColors();

    void Clear();

    /// <summary>Waits for a key press without echoing it. Ctrl+C is returned as a key instead of ending the program.</summary>
    ConsoleKeyInfo ReadKey();
}
