using CommanderD.Services;

namespace CommanderD.Commands;

public class MkDirCommand : AbstractCommand
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";
    private const string PathNotFoundMessage = "The system cannot find the path specified.";
    private const string AccessDeniedMessage = "Access is denied.";

    private readonly IFileSystemService fileSystemService;

    public MkDirCommand(IFileSystemService fileSystemService)
        : base("mkdir", "Creates a directory.")
    {
        this.fileSystemService = fileSystemService;
    }

    public override CommandResult Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            return CommandResult.Fail(SyntaxMessage);
        }

        List<string> errors = new List<string>();

        foreach (string argument in context.Arguments)
        {
            string? error = CreateDirectory(argument.Trim('"'));

            if (error != null)
            {
                errors.Add(error);
            }
        }

        if (errors.Count == 0)
        {
            return CommandResult.Ok();
        }

        return CommandResult.Fail(string.Join(Environment.NewLine, errors));
    }

    private string? CreateDirectory(string name)
    {
        if (name.Length == 0)
        {
            return SyntaxMessage;
        }

        try
        {
            string target = fileSystemService.ResolvePath(name);

            if (fileSystemService.DirectoryExists(target))
            {
                return $"A subdirectory or file {name} already exists.";
            }

            fileSystemService.CreateDirectory(target);
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return AccessDeniedMessage;
        }
        catch (Exception ex) when (ex is IOException
                                   or ArgumentException
                                   or NotSupportedException)
        {
            return PathNotFoundMessage;
        }
    }
}