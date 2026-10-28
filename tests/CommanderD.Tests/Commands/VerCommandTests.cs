using CommanderD.Commands;
using CommanderD.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class VerCommandTests
{
    
    private readonly CommandFactory _factory = new();
    private readonly IOutputWriter _outputWriter = new TestOutputWriter();
    
    [TestMethod]
    public void Given_shell_is_running_When_I_enter_ver_Then_the_current_version_is_returned()
    {

        var command = _factory.Create("ver", _outputWriter);
        var version = Environment.OSVersion;
        var versionString = version.ToString();
        
        Assert.IsNotNull(command);
        
        command.Execute(null);
        
        Assert.AreEqual(command.OutputWriter.output, versionString);
    }
}