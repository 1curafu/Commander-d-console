using CommanderD.Commands;

namespace CommanderD.Shell;

public class Parser
{
    private static readonly char[] Whitespace = { ' ', '\t' };
    private static readonly char[] NameEnd = { ' ', '\t', '.', '\\', '/' };

    public CommandContext? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        return new CommandContext(ParseCommand(input), ParseParameters(input));
    }

    public string ParseCommand(string input)
    {
        string line = input.Trim();
        return line.Substring(0, FindNameEnd(line));
    }

    public string[] ParseParameters(string input)
    {
        string line = input.Trim();
        string rest = line.Substring(FindNameEnd(line)).Trim();
        return rest.Split(Whitespace, StringSplitOptions.RemoveEmptyEntries);
    }

    private static int FindNameEnd(string line)
    {
        int end = line.IndexOfAny(NameEnd);

        if (end == 0)
        {
            end = line.IndexOfAny(Whitespace);
        }

        if (end == -1)
        {
            end = line.Length;
        }

        return end;
    }
}
