using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class ClsCommandTests
{
    private readonly FakeConsoleService _console = new FakeConsoleService();
    private readonly TestOutputWriter _outputWriter = new TestOutputWriter();
    private readonly ClsCommand _command;

    public ClsCommandTests()
    {
        _command = new ClsCommand(_console, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("cls", arguments));
    }

    [TestMethod]
    public void Cls_NoArgument_ClearsScreen()
    {
        Run();

        Assert.AreEqual(1, _console.ClearCalls);
    }

    [TestMethod]
    public void Cls_NoArgument_PrintsNothing()
    {
        Run();

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }
}
