using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class CommandInvokerTests
{
    private readonly FakeCommand fake = new FakeCommand();
    private readonly CommandInvoker invoker;

    public CommandInvokerTests()
    {
        var factory = new CommandFactory();
        factory.Register("fake", () => fake);
        invoker = new CommandInvoker(new CommandParser(), factory);
    }

    [TestMethod]
    public void ExecuteCommand_KnownCommand_ReturnsItsResult()
    {
        var result = invoker.ExecuteCommand("fake");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("fake output", result.OutputMessage);
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
        var result = invoker.ExecuteCommand("foo");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(
            "'foo' is not recognized as an internal or external command, operable program or batch file.",
            result.OutputMessage);
    }

    [TestMethod]
    public void ExecuteCommand_EmptyInput_DoesNothing()
    {
        var result = invoker.ExecuteCommand("");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("", result.OutputMessage);
        Assert.IsFalse(result.ShouldExit);
    }
}
