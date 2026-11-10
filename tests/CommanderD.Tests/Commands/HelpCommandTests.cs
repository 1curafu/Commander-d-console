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
    public void Help_NewCommandInFactory_AppearsInTheList()
    {
        _factory.Register("fake", outputWriter => new FakeCommand("fake", outputWriter));

        Run();

        StringAssert.Contains(_outputWriter.OutputBuffer, "FAKE      fake command");
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
    public void Help_UnknownCommand_ShowsNotSupportedMessage()
    {
        Run("gibtsnicht");

        StringAssert.Contains(_outputWriter.OutputBuffer, "This command is not supported by the help utility.");
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

    [TestMethod]
    [DataRow("type", "TYPE <file>")]
    [DataRow("ren", "REN <name> <new name>")]
    [DataRow("rename", "REN <name> <new name>")]
    [DataRow("move", "MOVE <source>[,<source>...] <destination>")]
    [DataRow("more", "MORE <file>")]
    public void Help_WithFileCommands_ShowsDescriptionAndUsage(string name, string usage)
    {
        Run(name);

        Command command = _factory.Create(name, _outputWriter)!;
        StringAssert.Contains(_outputWriter.OutputBuffer, command.Description);
        StringAssert.Contains(_outputWriter.OutputBuffer, usage);
    }
}