using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class ClsCommandTests
{
    private readonly FakeConsoleService _console = new FakeConsoleService();
    private readonly TestOutputWriter _outputWriter = new TestOutputWriter();
    private readonly ClsCommand _command;

    public ClsCommandTests()
    {
        _command = new ClsCommand(_console, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("cls", arguments));
    }

    [TestMethod]
    public void Cls_HasNameAndDescription()
    {
        Assert.AreEqual("cls", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }

    [TestMethod]
    public void Cls_NoArgument_ClearsScreen()
    {
        Run();

        Assert.AreEqual(1, _console.ClearCalls);
    }

    [TestMethod]
    public void Cls_NoArgument_PrintsNothing()
    {
        Run();

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Cls_WithArguments_StillClearsScreen()
    {
        Run("extra", "arguments");

        Assert.AreEqual(1, _console.ClearCalls);
        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Cls_DoesNotChangeColors()
    {
        _console.SetColors(ConsoleColor.DarkBlue, ConsoleColor.Yellow);

        Run();

        Assert.AreEqual(ConsoleColor.DarkBlue, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Yellow, _console.ForegroundColor);
        Assert.AreEqual(0, _console.ResetColorsCalls);
    }

    [TestMethod]
    public void Cls_CalledTwice_ClearsTwice()
    {
        Run();
        Run();

        Assert.AreEqual(2, _console.ClearCalls);
    }

    // ---- Through parser, factory and invoker (like typing it into the shell) ----

    [TestMethod]
    [DataRow("cls")]
    [DataRow("CLS")]
    [DataRow("Cls")]
    public void Cls_TypedIntoShell_ClearsScreenAndPrintsNothing(string input)
    {
        var writer = new TestOutputWriter();
        var factory = new CommandFactory();
        factory.Register("cls", (outputWriter) => new ClsCommand(_console, outputWriter));
        var invoker = new CommandInvoker(new Parser(), factory, writer);

        invoker.ExecuteCommand(input);

        Assert.AreEqual(1, _console.ClearCalls);
        Assert.AreEqual(string.Empty, writer.OutputBuffer);
    }

    [TestMethod]
    public void Cls_RegisteredInDefaultFactory()
    {
        var factory = new CommandFactory();

        Assert.IsInstanceOfType(factory.Create("CLS", new TestOutputWriter()), typeof(ClsCommand));
    }
}
