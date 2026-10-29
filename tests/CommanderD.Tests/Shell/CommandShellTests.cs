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
        var writer = new TestOutputWriter();
        var factory = new CommandFactory();
        factory.Register("hello", (outputWriter) => new FakeCommand("hello",outputWriter));
        factory.Register("exit", (outputWriter) => new FakeCommand("exit",outputWriter));

        var reader = new StringReader(input);
        Console.SetIn(reader);

        var shell = new CommandShell(new CommandInvoker(new CommandParser(), factory, writer));
        shell.Run();

        return writer.output;
    }

    [TestMethod]
    public void Run_KnownCommand_PrintsOutput()
    {
        var output = RunShell("hello\n");

        StringAssert.Contains(output, "hello");
    }

    [TestMethod]
    public void Run_UnknownCommand_PrintsMessageAndKeepsRunning()
    {
        var output = RunShell("foo\n");

        StringAssert.Contains(output, "'foo' is not recognized as an internal or external command, operable program or batch file.");

        output = RunShell("hello\n");
        
        StringAssert.Contains(output, "hello");
    }

    [TestMethod]
    public void Run_CommandNameIsCaseInsensitive()
    {
        var output = RunShell("HELLO\n");

        StringAssert.Contains(output, "hello");
    }
}
