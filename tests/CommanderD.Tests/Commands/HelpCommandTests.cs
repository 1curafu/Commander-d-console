using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class HelpCommandTests
{
    private readonly TestOutputWriter _outputWriter = new();
    private readonly CommandFactory _factory = new();
    private readonly HelpCommand _command;

    public HelpCommandTests()
    {
        _command = new HelpCommand(_factory, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("help", arguments));
    }

    private string[] Lines()
    {
        return _outputWriter.OutputBuffer.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    }

    [TestMethod]
    public void Help_HasNameAndDescription()
    {
        Assert.AreEqual("help", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }

    [TestMethod]
    public void Help_NoArgument_ShowsHintHowToGetDetails()
    {
        Run();

        StringAssert.Contains(_outputWriter.OutputBuffer, "HELP <command>");
    }

    [TestMethod]
    [DataRow("HELP")]
    [DataRow("EXIT")]
    [DataRow("CD")]
    [DataRow("VER")]
    [DataRow("MKDIR")]
    [DataRow("DIR")]
    [DataRow("DEL")]
    [DataRow("COLOR")]
    [DataRow("CLS")]
    public void Help_NoArgument_ListsEveryCommandWithItsDescription(string name)
    {
        Run();

        Command command = _factory.Create(name, _outputWriter)!;
        StringAssert.Contains(_outputWriter.OutputBuffer, $"{name.PadRight(10)}{command.Description}");
    }

    [TestMethod]
    public void Help_NoArgument_ListsACommandWithAliasOnlyOnce()
    {
        Run();

        int mkdirLines = Lines().Count(line => line.StartsWith("MKDIR") || line.StartsWith("MD "));
        Assert.AreEqual(1, mkdirLines);
    }

    [TestMethod]
    public void Help_NewCommandInFactory_AppearsInTheList()
    {
        _factory.Register("fake", outputWriter => new FakeCommand("fake", outputWriter));

        Run();

        StringAssert.Contains(_outputWriter.OutputBuffer, "FAKE      fake command");
    }

    [TestMethod]
    public void Help_NoArgument_ListsCommandsSortedByName()
    {
        Run();

        string[] names = Lines().Skip(1).Select(line => line.Substring(0, 10).Trim()).ToArray();
        CollectionAssert.AreEqual(names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray(), names);
    }

    [TestMethod]
    public void Help_WithCommandName_ShowsDescriptionAndUsage()
    {
        Run("cd");

        Command cd = _factory.Create("cd", _outputWriter)!;
        _outputWriter.ClearOutputBuffer();
        Run("cd");

        StringAssert.Contains(_outputWriter.OutputBuffer, cd.Description);
        StringAssert.Contains(_outputWriter.OutputBuffer, "CD [path]");
    }

    [TestMethod]
    [DataRow("cd")]
    [DataRow("CD")]
    [DataRow("Cd")]
    public void Help_CommandNameIsNotCaseSensitive(string name)
    {
        Run(name);

        StringAssert.Contains(_outputWriter.OutputBuffer, "CD [path]");
    }

    [TestMethod]
    public void Help_WithCommandNameInQuotes_RemovesQuotes()
    {
        Run("\"cd\"");

        StringAssert.Contains(_outputWriter.OutputBuffer, "CD [path]");
    }

    [TestMethod]
    public void Help_WithAlias_ShowsTheCommand()
    {
        Run("md");

        StringAssert.Contains(_outputWriter.OutputBuffer, "MKDIR <name>");
    }

    [TestMethod]
    public void Help_WithCommandWithoutArguments_ShowsNameAsUsage()
    {
        Run("ver");

        StringAssert.Contains(_outputWriter.OutputBuffer, "VER");
        StringAssert.Contains(_outputWriter.OutputBuffer, "Shows the version number.");
    }

    [TestMethod]
    public void Help_WithHelp_ShowsOwnUsage()
    {
        Run("help");

        StringAssert.Contains(_outputWriter.OutputBuffer, "HELP <command>");
    }

    [TestMethod]
    public void Help_UnknownCommand_ShowsNotSupportedMessage()
    {
        Run("gibtsnicht");

        StringAssert.Contains(_outputWriter.OutputBuffer, "This command is not supported by the help utility.");
    }

    [TestMethod]
    public void Help_MoreThanOneArgument_UsesTheFirstOne()
    {
        Run("cd", "gibtsnicht");

        StringAssert.Contains(_outputWriter.OutputBuffer, "CD [path]");
        Assert.IsFalse(_outputWriter.OutputBuffer.Contains("not supported"));
    }

    [TestMethod]
    public void Factory_HelpIsRegistered()
    {
        Assert.IsInstanceOfType(_factory.Create("help", _outputWriter), typeof(HelpCommand));
        Assert.IsInstanceOfType(_factory.Create("HELP", _outputWriter), typeof(HelpCommand));
    }

    [TestMethod]
    public void Usage_CommandWithoutOverride_IsTheNameInUpperCase()
    {
        var command = new FakeCommand("fake", _outputWriter);

        Assert.AreEqual("FAKE", command.Usage);
    }

    [TestMethod]
    public void Help_WithColor_ShowsTheAvailableColors()
    {
        Run("color");

        StringAssert.Contains(_outputWriter.OutputBuffer, "Available colors:");

        foreach (string name in ColorPalette.Names)
        {
            StringAssert.Contains(_outputWriter.OutputBuffer, name);
        }
    }
}
