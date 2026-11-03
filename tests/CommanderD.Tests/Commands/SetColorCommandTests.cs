using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class SetColorCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";

    private readonly FakeConsoleService _console = new FakeConsoleService();
    private readonly TestOutputWriter _outputWriter = new TestOutputWriter();
    private readonly SetColorCommand _command;

    public SetColorCommandTests()
    {
        _command = new SetColorCommand(_console, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("color", arguments));
    }

    [TestMethod]
    public void Color_HasNameAndDescription()
    {
        Assert.AreEqual("color", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }


    [TestMethod]
    public void Color_Green_ChangesOnlyForeground()
    {
        Run("green");

        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Green, _console.ForegroundColor);
    }

    [TestMethod]
    [DataRow("white", "on", "darkblue", ConsoleColor.DarkBlue, ConsoleColor.White)]
    [DataRow("black", "on", "white", ConsoleColor.White, ConsoleColor.Black)]
    [DataRow("yellow", "on", "darkred", ConsoleColor.DarkRed, ConsoleColor.Yellow)]
    [DataRow("red", "on", "darkgreen", ConsoleColor.DarkGreen, ConsoleColor.Red)]
    public void Color_ForegroundOnBackground_SetsBoth(string foreground, string keyword, string background, ConsoleColor expectedBackground, ConsoleColor expectedForeground)
    {
        Run(foreground, keyword, background);

        Assert.AreEqual(expectedBackground, _console.BackgroundColor);
        Assert.AreEqual(expectedForeground, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_IsCaseInsensitive()
    {
        Run("YELLOW", "On", "DarkBlue");

        Assert.AreEqual(ConsoleColor.DarkBlue, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Yellow, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_GreyIsAliasForGray()
    {
        Run("green");
        Run("grey");

        Assert.AreEqual(ConsoleColor.Gray, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_ForegroundOnly_KeepsCurrentBackground()
    {
        Run("white", "on", "darkblue");
        Run("green");

        Assert.AreEqual(ConsoleColor.DarkBlue, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Green, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_NoArgument_RestoresDefaultColors()
    {
        Run("white", "on", "darkblue");

        Run();

        Assert.AreEqual(1, _console.ResetColorsCalls);
        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Gray, _console.ForegroundColor);
    }


    [TestMethod]
    public void Color_ValidName_AppliesColorsOnceAndPrintsNothing()
    {
        Run("green");

        Assert.AreEqual(1, _console.SetColorsCalls);
        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }


    [TestMethod]
    public void Color_SameBackgroundAndForeground_IsPrevented()
    {
        Run("green", "on", "green");

        Assert.AreEqual(0, _console.SetColorsCalls);
        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Gray, _console.ForegroundColor);
        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Color_ForegroundEqualToCurrentBackground_IsPrevented()
    {
        Run("black");

        Assert.AreEqual(0, _console.SetColorsCalls);
    }


    [TestMethod]
    [DataRow("purple")]
    [DataRow("1F")]
    [DataRow("0A")]
    [DataRow("gren")]
    public void Color_UnknownName_ShowsSyntaxMessageAndChangesNothing(string name)
    {
        Run(name);

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _console.SetColorsCalls);
    }

    [TestMethod]
    public void Color_UnknownBackground_ShowsSyntaxMessageAndChangesNothing()
    {
        Run("green", "on", "purple");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _console.SetColorsCalls);
    }

    [TestMethod]
    [DataRow("green blue")]
    [DataRow("green on")]
    [DataRow("green to blue")]
    [DataRow("green on blue red")]
    public void Color_WrongArgumentShape_ShowsSyntaxMessage(string arguments)
    {
        Run(arguments.Split(' '));

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _console.SetColorsCalls);
    }


    private CommandInvoker CreateInvoker(TestOutputWriter writer)
    {
        var factory = new CommandFactory();
        factory.Register("color", (outputWriter) => new SetColorCommand(_console, outputWriter));
        return new CommandInvoker(new Parser(), factory, writer);
    }

    [TestMethod]
    [DataRow("color green")]
    [DataRow("COLOR GREEN")]
    [DataRow("Color Green")]
    public void Color_TypedIntoShell_ChangesColor(string input)
    {
        var writer = new TestOutputWriter();

        CreateInvoker(writer).ExecuteCommand(input);

        Assert.AreEqual(string.Empty, writer.OutputBuffer);
        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Green, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_RegisteredInDefaultFactory()
    {
        var factory = new CommandFactory();

        Assert.IsInstanceOfType(factory.Create("COLOR", new TestOutputWriter()), typeof(SetColorCommand));
    }
}