using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class ClsCommand(IConsoleService consoleService, IOutputWriter outputWriter)
    : Command("cls", "Clears the screen.", outputWriter)
{
    public override void Execute(CommandContext context)
    {
        consoleService.Clear();
    }
}
