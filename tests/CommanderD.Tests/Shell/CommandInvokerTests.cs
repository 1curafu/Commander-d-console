using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class CommandInvokerTests
{
    private FakeCommand _fakeCommand;
    private readonly CommandInvoker _invoker;
    private readonly IOutputWriter _outputWriter = new TestOutputWriter();


    public CommandInvokerTests()
    {
        var factory = new CommandFactory();
        factory.Register("fake", (outputWriter) => new FakeCommand("fake", outputWriter));
        _invoker = new CommandInvoker(new CommandParser(), factory, _outputWriter);
    }

    [TestMethod]
    public void ExecuteCommand_KnownCommand_ReturnsItsResult()
    {
        var command = _invoker.ExecuteCommand("fake");
        Assert.IsNotNull(command);
        Assert.AreEqual("fake", command.OutputWriter.output);
    }

    [TestMethod]
    public void ExecuteCommand_KnownCommand_PassesArguments()
    {
        var command = (FakeCommand)_invoker.ExecuteCommand("fake one two");
        
        Assert.IsNotNull(command.LastContext);
        CollectionAssert.AreEqual(new[] { "one", "two" }, command.LastContext.Arguments);
    }

    [TestMethod]
    public void ExecuteCommand_UnknownCommand_ReturnsNotRecognizedMessage()
    {
        var command = _invoker.ExecuteCommand("foo");
        
        Assert.IsNull(command);
        Assert.AreEqual("'foo' is not recognized as an internal or external command, operable program or batch file.", _invoker.OutputWriter.output);
    }

    [TestMethod]
    public void ExecuteCommand_EmptyInput_DoesNothing()
    {
        var command = _invoker.ExecuteCommand("");
        Assert.IsNull(command);
    }
}
