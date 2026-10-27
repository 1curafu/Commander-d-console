using CommanderD.Commands;

namespace CommanderD.Shell;

public class CommandParser
{
    private static readonly char[] Whitespace = { ' ', '\t' };
    private static readonly char[] NameEnd = { ' ', '\t', '.', '\\', '/' };

    public CommandContext? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string line = input.Trim();
        int end = line.IndexOfAny(NameEnd);

        if (end == 0)
        {
            end = line.IndexOfAny(Whitespace);
        }

        if (end == -1)
        {
            end = line.Length;
        }

        string name = line.Substring(0, end);
        string rest = line.Substring(end).Trim();
        string[] arguments = rest.Split(Whitespace, StringSplitOptions.RemoveEmptyEntries);

        return new CommandContext(name, arguments);
    }
}