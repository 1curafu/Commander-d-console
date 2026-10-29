using CommanderD.Commands;
using CommanderD.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class ExitCommandTests
{
    private readonly ExitCommand _command = new(new TestOutputWriter());

    [TestMethod]
    public void Exit_HasNameAndDescription()
    {
        Assert.AreEqual("exit", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }

    [TestMethod]
    public void Exit_BeforeExecute_DoesNotRequestExit()
    {
        Assert.IsFalse(_command.ShouldExit);
    }

    [TestMethod]
    public void Exit_Execute_RequestsExit()
    {
        _command.Execute(new CommandContext("exit", Array.Empty<string>()));

        Assert.IsTrue(_command.ShouldExit);
    }

    [TestMethod]
    public void Exit_IsFoundByFactoryCaseInsensitive()
    {
        var command = new CommandFactory().Create("EXIT", new TestOutputWriter());

        Assert.IsInstanceOfType<ExitCommand>(command);
    }
}
