using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class CommandInvokerTests
{
    private readonly FakeCommand fake = new();
    private readonly CommandInvoker invoker;

    public CommandInvokerTests()
    {
        var factory = new CommandFactory();
        factory.Register("fake", () => fake);
        invoker = new CommandInvoker(new CommandParser(), factory, new TestOutputWriter());
    }

    [TestMethod]
    public void ExecuteCommand_KnownCommand_ReturnsItsResult()
    {
        var command = invoker.ExecuteCommand("fake");
        Assert.IsNotNull(command);
        Assert.AreEqual("fake", command.OutputWriter.output);
    }

    [TestMethod]
    public void ExecuteCommand_KnownCommand_PassesArguments()
    {
        invoker.ExecuteCommand("fake one two");

        Assert.IsNotNull(fake.LastContext);
        CollectionAssert.AreEqual(new[] { "one", "two" }, fake.LastContext.Arguments);
    }

    [TestMethod]
    public void ExecuteCommand_UnknownCommand_ReturnsNotRecognizedMessage()
    {
        var command = invoker.ExecuteCommand("foo");
        
        Assert.IsNull(command);
        Assert.AreEqual("'foo' is not recognized as an internal or external command, operable program or batch file.", invoker.outputWriter.output);
    }

    [TestMethod]
    public void ExecuteCommand_EmptyInput_DoesNothing()
    {
        var command = invoker.ExecuteCommand("");
        Assert.IsNull(command);
    }
}
