using CommanderD.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class VerCommandTests
{
    [TestMethod]
    public void Given_shell_is_running_When_I_enter_ver_Then_the_current_version_is_returned()
    {
        var verCommand = new VerCommand();
        var commandContext = new CommandContext(null, null);
        var version = Environment.OSVersion;
        var versionString = version.ToString();
        
        var commandResult = verCommand.Execute(commandContext);
        
        Assert.AreEqual(commandResult.OutputMessage, versionString);
    }
}