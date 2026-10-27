using CommanderD.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class CommandResultTests
{
    [TestMethod]
    public void Ok_IsSuccessfulAndDoesNotExit()
    {
        var result = CommandResult.Ok("done");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("done", result.OutputMessage);
        Assert.IsFalse(result.ShouldExit);
    }

    [TestMethod]
    public void Fail_IsNotSuccessful()
    {
        var result = CommandResult.Fail("error");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("error", result.OutputMessage);
        Assert.IsFalse(result.ShouldExit);
    }

    [TestMethod]
    public void Exit_SetsShouldExit()
    {
        var result = CommandResult.Exit();

        Assert.IsTrue(result.ShouldExit);
    }
}
