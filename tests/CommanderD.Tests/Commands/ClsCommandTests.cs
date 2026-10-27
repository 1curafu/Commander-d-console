using CommanderD.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ClsCommandTests
{
    [TestMethod]
    public void Given_shell_has_text_When_I_enter_cls_Then_the_shell_gets_cleared()
    {
        var clsCommand = new ClsCommand();
        var commandContext = new CommandContext(null, null);
        
        var commandResult = clsCommand.Execute(commandContext);
        
        Assert.AreEqual(commandResult.OutputMessage, "");
    }
}