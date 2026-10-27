# How to add a command

The foundation (CD-14) provides the shell, the parser, the factory, the services and
`AbstractCommand`. A command class is not part of it. Every command story creates its
own class and registers it in the `CommandFactory`. The empty file
`src/CommanderD/Commands/<Name>Command.cs` of your story already exists.

## 1. Branch

```
git switch main
git pull
git switch -c feature/CD-<number>-<short-name>
```

## 2. Create the class

File `src/CommanderD/Commands/<Name>Command.cs`. The class extends
`AbstractCommand` and implements `Execute`:

```csharp
using CommanderD.Services;

namespace CommanderD.Commands;

public class ExampleCommand : AbstractCommand
{
    public ExampleCommand(IFileSystemService fileService, IConsoleService consoleService)
        : base("example", "Shows how a command looks.", fileService, consoleService)
    {
    }

    public override CommandResult Execute(CommandContext context)
    {
        if (context.Arguments.Count == 0)
        {
            return CommandResult.Fail("The syntax of the command is incorrect.");
        }

        return CommandResult.Ok($"You wrote: {context.Arguments[0]}");
    }
}
```

- `context.Arguments` holds everything after the command name.
- Use `FileService` and `ConsoleService` for all file and console access. Never
  call `File`, `Directory` or `Console` directly in a command.

## 3. Register it in the factory

In `CommandFactory` add exactly **one line** for your command:

```csharp
["example"] = () => new ExampleCommand(_fileService, _consoleService),
```

## 4. Services

A method of `IFileSystemService` or `IConsoleService` that throws
`NotImplementedException("CD-<number>")` belongs to your story. Implement it in
`FileSystemService` or `ConsoleService`. If you need a new method, add it to the
interface and the class and mention it in the pull request.

## 5. Result

Return `CommandResult.Ok(...)`, `CommandResult.Fail(...)` or `CommandResult.Exit()`.
The shell prints `OutputMessage`, also for errors. Do not print it yourself.

## 6. Unit tests

Your test class already exists, empty, in `tests/CommanderD.Tests/Commands`. Write
one test per acceptance criterion of your story, as described in
[testing.md](testing.md) and run `dotnet test`.

## Test it

Start the shell, run your command, and run the same command in the real `cmd.exe`.
Output and error messages must be the same. Then open a pull request, see
[git-workflow.md](git-workflow.md).
