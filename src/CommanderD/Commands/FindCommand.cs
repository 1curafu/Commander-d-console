using System.Text;
using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class FindCommand : Command
{
    private readonly IFileSystemService _fileSystemService;

    public FindCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("find", "Shows the full path of every match in the current directory and its subdirectories.", outputWriter)
    {
        _fileSystemService = fileSystemService;
    }

    public override string Usage => "FIND <name>";

    public override void Execute(CommandContext context)
    {
        string name = string.Join(" ", context.Arguments).Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(name))
        {
            OutputWriter.WriteLn("The syntax of the command is incorrect.");
            return;
        }

        try
        {
            var matches = new List<string>();
            Search(_fileSystemService.CurrentDirectory, name, matches);

            if (matches.Count == 0)
            {
                OutputWriter.WriteLn($"File not found - {name}");
                return;
            }

           

            var output = new StringBuilder();
            foreach (string match in matches)
            {
                output.AppendLine(match);
            }

            OutputWriter.Write(output.ToString());
        }
        catch (UnauthorizedAccessException)
        {
            OutputWriter.WriteLn("Access is denied.");
        }
        catch (IOException e)
        {
            OutputWriter.WriteLn($"Error while searching: {e.Message}");
        }
    }

    private void Search(string directory, string name, List<string> matches)
    {
        foreach (DirectoryEntry entry in _fileSystemService.GetEntries(directory))
        {
            string fullPath = $"{directory.TrimEnd('\\')}\\{entry.Name}";

            if (string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(fullPath);
            }

            if (entry.IsDirectory)
            {
                Search(fullPath, name, matches);
            }
        }
    }
}
