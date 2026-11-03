using CommanderD.Services;

namespace CommanderD.Tests.Fakes;

public class FakeConsoleService : IConsoleService
{
    public ConsoleColor BackgroundColor { get; private set; } = ConsoleColor.Black;

    public ConsoleColor ForegroundColor { get; private set; } = ConsoleColor.Gray;

    public int SetColorsCalls { get; private set; }

    public int ResetColorsCalls { get; private set; }

    public void SetColors(ConsoleColor background, ConsoleColor foreground)
    {
        SetColorsCalls++;
        BackgroundColor = background;
        ForegroundColor = foreground;
    }

    public void ResetColors()
    {
        ResetColorsCalls++;
        BackgroundColor = ConsoleColor.Black;
        ForegroundColor = ConsoleColor.Gray;
    }
}