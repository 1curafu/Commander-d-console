using CommanderD.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class VerCommandTests
{
    
    private readonly CommandFactory _factory = new CommandFactory();
    
    [TestMethod]
    public void Given_shell_is_running_When_I_enter_ver_Then_the_current_version_is_returned()
    {

        var command = _factory.Create("ver");
        var version = Environment.OSVersion;
        var versionString = version.ToString();
        
        Assert.AreEqual(commandResult.OutputMessage, versionString);
    }
}