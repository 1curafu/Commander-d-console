using System;
using System.IO;
using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class CommandShellTests
{
    private static string RunShell(string input)
    {
        var factory = new CommandFactory();
        factory.Register("hello", () => new FakeCommand("hello", CommandResult.Ok("hello-output")));
        factory.Register("exit", () => new FakeCommand("exit", CommandResult.Exit()));

        var reader = new StringReader(input);
        var writer = new StringWriter();
        Console.SetIn(reader);
        Console.SetOut(writer);

        var shell = new CommandShell(new CommandInvoker(new CommandParser(), factory));
        shell.Run();

        return writer.ToString();
    }

    [TestMethod]
    public void Run_KnownCommand_PrintsOutput()
    {
        string output = RunShell("hello\n");

        StringAssert.Contains(output, "hello-output");
    }

    [TestMethod]
    public void Run_UnknownCommand_PrintsMessageAndKeepsRunning()
    {
        string output = RunShell("foo\nhello\n");

        StringAssert.Contains(output, "'foo' is not recognized as an internal or external command, operable program or batch file.");
        StringAssert.Contains(output, "hello-output");
    }

    [TestMethod]
    public void Run_EmptyInput_ShowsPromptAgain()
    {
        string output = RunShell("\n\n");

        Assert.AreEqual(3, output.Split('>').Length - 1);
    }

    [TestMethod]
    public void Run_ExitCommand_StopsShell()
    {
        string output = RunShell("exit\nhello\n");

        Assert.IsFalse(output.Contains("hello-output"));
    }

    [TestMethod]
    public void Run_CommandNameIsCaseInsensitive()
    {
        string output = RunShell("HELLO\n");

        StringAssert.Contains(output, "hello-output");
    }
}
