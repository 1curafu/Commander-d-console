using CommanderD.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ExitCommandTests
{
    [TestMethod]
    public void Given_shell_is_running_When_I_enter_exit_Then_the_shell_ends()
    {
        var _exitCommand = new ExitCommand();
        var _commandContext = new CommandContext(null, null);
    
        var _commandResult = _exitCommand.Execute(_commandContext);

        Assert.AreEqual(_commandResult.Success, true);
        Assert.AreEqual(_commandResult.ShouldExit, true);
    }
}