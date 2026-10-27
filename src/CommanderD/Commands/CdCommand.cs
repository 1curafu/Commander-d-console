using CommanderD.Services;

namespace CommanderD.Commands
{
    public class CdCommand : AbstractCommand
    {
        private readonly IFileSystemService _fileSystemService;

        public CdCommand(IFileSystemService fileSystemService) 
            : base("cd", "Changes the current working directory.")
        {
            _fileSystemService = fileSystemService;
        }

        public override CommandResult Execute(CommandContext context)
        {
            if (context.Arguments == null || context.Arguments.Count == 0)
            {
                var currentDir = _fileSystemService.GetCurrentDirectory();
                return CommandResult.Ok(currentDir);
            }

            var arg = context.Arguments[0];

            if (arg.Equals("cd..", StringComparison.OrdinalIgnoreCase) || arg.Equals("cd..", StringComparison.OrdinalIgnoreCase))
            {
                arg = "..";
            }
            else if (arg.Equals("cd\\", StringComparison.OrdinalIgnoreCase))
            {
                arg = "\\";
            }

            try
            {
                if (_fileSystemService.DirectoryExists(arg) || arg == ".." || arg == "\\")
                {
                    _fileSystemService.ChangeDirectory(arg);
                    return CommandResult.Ok(string.Empty);
                }
                else
                {
                    return CommandResult.Fail("The system cannot find the path specified.");
                }
            }
            catch
            {
                return CommandResult.Fail("The system cannot find the path specified.");
            }
        }
    }
}