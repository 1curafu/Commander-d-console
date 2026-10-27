namespace CommanderD.Commands;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    CommandResult Execute(CommandContext context);
}
