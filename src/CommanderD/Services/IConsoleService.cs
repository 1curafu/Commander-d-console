namespace CommanderD.Services;

public interface IConsoleService
{
    ConsoleColor BackgroundColor { get; }

    ConsoleColor ForegroundColor { get; }

    void SetColors(ConsoleColor background, ConsoleColor foreground);

    void ResetColors();
}