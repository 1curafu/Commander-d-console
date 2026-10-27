# Testing

Team D uses xUnit. The tests are in `tests/CommanderD.Tests`.

## Run

```
dotnet test
```

## Where the tests are

- `tests/CommanderD.Tests/Commands/<Name>CommandTests.cs` has one class per command.
- `tests/CommanderD.Tests/Shell` has the tests of the parser, the factory and the shell.
- `tests/CommanderD.Tests/Services` has the tests of the services that contain logic.
- `tests/CommanderD.Tests/Fakes` has fake services, so a test never touches the real file system or console.

## Write the tests of your story

Every acceptance criterion of the story gets at least one test in your test class.

1. Write one `[Fact]` per criterion.
2. Arrange the fake services, call `Execute` with a `CommandContext`, check the `CommandResult`.
3. Run `dotnet test` until it passes.

## Example

```csharp
[Fact]
public void Example_NoArgument_ShowsSyntaxMessage()
{
    var command = new ExampleCommand(new FakeFileSystemService(), new FakeConsoleService());
    var context = new CommandContext("example", "example", new List<string>());

    var result = command.Execute(context);

    Assert.False(result.Success);
    Assert.Equal("The syntax of the command is incorrect.", result.OutputMessage);
}
```
