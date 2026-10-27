# Testing

Team D uses MSTest. The tests are in `tests/CommanderD.Tests`.

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

1. Write one `[TestMethod]` per criterion.
2. Arrange the fake services, call `Execute` with a `CommandContext`, check the `CommandResult`.
3. Run `dotnet test` until it passes.

## Example

```csharp
[TestClass]
public class ExampleCommandTests
{
    [TestMethod]
    public void Example_NoArgument_ShowsSyntaxMessage()
    {
        var command = new ExampleCommand();
        var context = new CommandContext("example", new string[0]);

        var result = command.Execute(context);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("The syntax of the command is incorrect.", result.OutputMessage);
    }
}
```

Several inputs for the same test: add `[DataRow(...)]` lines under `[TestMethod]` and give the method matching parameters. Compare arrays with `CollectionAssert.AreEqual`.
