using CommanderD.Services;

namespace CommanderD.Tests.Fakes;

public class FakeConsoleService : IConsoleService
{
    private readonly Queue<ConsoleKeyInfo> _keys = new();

    public ConsoleColor BackgroundColor { get; private set; } = ConsoleColor.Black;

    public ConsoleColor ForegroundColor { get; private set; } = ConsoleColor.Gray;

    public int WindowHeight { get; set; } = 25;

    public int SetColorsCalls { get; private set; }

    public int ResetColorsCalls { get; private set; }

    public int ClearCalls { get; private set; }

    public int ReadKeyCalls { get; private set; }

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

    public void Clear()
    {
        ClearCalls++;
    }

    /// <summary>Queues key presses that <see cref="ReadKey"/> will return in order.</summary>
    public void PressKeys(params ConsoleKeyInfo[] keys)
    {
        foreach (ConsoleKeyInfo key in keys)
        {
            _keys.Enqueue(key);
        }
    }

    /// <summary>Returns the next queued key. When none are left, Q is returned so a test can never hang.</summary>
    public ConsoleKeyInfo ReadKey()
    {
        ReadKeyCalls++;

        return _keys.Count > 0
            ? _keys.Dequeue()
            : new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false);
    }
}
