using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class PromptTests
{
    private static string RunShell(string input, FakeFileSystemService fileSystem, CommandFactory factory)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var console = new StringWriter();

        try
        {
            Console.SetIn(new StringReader(input));
            Console.SetOut(console);

            var invoker = new CommandInvoker(new CommandParser(), factory, new TestOutputWriter());
            var shell = new CommandShell(invoker, fileSystem);
            shell.Run();
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }

        return console.ToString();
    }

    [TestMethod]
    public void Prompt_AtStart_ShowsCurrentDirectoryFollowedByGreaterThan()
    {
        var fileSystem = new FakeFileSystemService(@"C:\Users\Name");

        string output = RunShell("", fileSystem, new CommandFactory());

        Assert.AreEqual(@"C:\Users\Name>", output);
    }

    [TestMethod]
    public void Prompt_EmptyInput_ShowsSamePromptAgain()
    {
        var fileSystem = new FakeFileSystemService(@"C:\Users\Name");

        string output = RunShell("\n", fileSystem, new CommandFactory());

        Assert.AreEqual(@"C:\Users\Name>C:\Users\Name>", output);
    }

    [TestMethod]
    public void Prompt_AfterCd_ShowsNewDirectory()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-prompt-tests"));
        string start = Path.Combine(root, "work");
        string src = Path.Combine(start, "src");

        var fileSystem = new FakeFileSystemService(start);
        fileSystem.AddDirectory(src);

        var factory = new CommandFactory();
        factory.Register("cd", outputWriter => new CdCommand(fileSystem, outputWriter));

        string output = RunShell("cd src\n", fileSystem, factory);

        Assert.AreEqual(start + ">" + src + ">", output);
    }

    [TestMethod]
    public void Prompt_AfterExit_IsNotShownAgain()
    {
        var fileSystem = new FakeFileSystemService(@"C:\Users\Name");

        string output = RunShell("exit\n\n\n", fileSystem, new CommandFactory());

        Assert.AreEqual(@"C:\Users\Name>", output);
    }
}
