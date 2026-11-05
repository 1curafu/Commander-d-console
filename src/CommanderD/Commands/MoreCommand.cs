using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class MoreCommand : Command
{
    private const string SyntaxErrorMessage = "The syntax of the Command is incorrect.";

    private readonly IFileSystemService _fileSystemService;
    private readonly IOutputWriter _outputWriter;

    public MoreCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("more", "Displays the contents of a file.", outputWriter)
    {
        _fileSystemService = fileSystemService;
        _outputWriter = outputWriter;
    }

    public override void Execute(CommandContext context)
    {
        string name = string.Join(" ", context.Arguments).Trim('"');

        if (name.Length == 0)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        string target;

        try
        {
            target = _fileSystemService.ResolvePath(name);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        if (!_fileSystemService.FileExists(target))
        {
            _outputWriter.WriteLn($"Cannot access file {target}");
            return;
        }

        string content = _fileSystemService.ReadFile(target);

        if (content.Length == 0)
        {
            return;
        }

        foreach (string line in content.Split('\n'))
        {
            _outputWriter.WriteLn(line.TrimEnd('\r'));
        }
    }
}
