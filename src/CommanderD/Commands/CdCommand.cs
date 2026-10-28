using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class CdCommand : AbstractCommand
{
    private const string PathNotFoundMessage = "The system cannot find the path specified.";

    private readonly IFileSystemService fileSystemService;

    public CdCommand(IFileSystemService fileSystemService)
        : base("cd", "Displays or changes the current directory.")
    {
        this.fileSystemService = fileSystemService;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            //return CommandResult.Ok(fileSystemService.CurrentDirectory);
        }

        string path = string.Join(" ", context.Arguments).Trim('"');

        try
        {
            string target = fileSystemService.ResolvePath(path);

            if (!fileSystemService.DirectoryExists(target))
            {
                //return CommandResult.Fail(PathNotFoundMessage);
            }

            fileSystemService.ChangeDirectory(target);
            //return CommandResult.Ok();
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or ArgumentException
                                   or NotSupportedException)
        {
            //return CommandResult.Fail(PathNotFoundMessage);
        }
    }
}
